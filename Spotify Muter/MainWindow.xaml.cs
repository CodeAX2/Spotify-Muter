using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using VideoPlayerController;
using System.IO;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using DesktopNotifications;
using System.Configuration;
using System.Data;



namespace Spotify_Muter {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 
	public partial class MainWindow : Window {

		private Label songLbl, artistLbl;
		private List<string> artistBlacklist;

		public MainWindow() {

			InitializeComponent();
			Title = "Spotify Muter";
			ResizeMode = ResizeMode.CanMinimize;

			// Get the label
			songLbl = (Label)this.FindName("Song");
			artistLbl = (Label)this.FindName("Artist");
			try {
				artistBlacklist = System.IO.File.ReadAllLines("blacklist.txt").ToList();
			} catch (Exception e) {
				// File is corrupt, does not exist, etc.
				System.IO.File.Create("blacklist.txt");
				artistBlacklist = new List<string>();
			}

			// Remove empty artists from the blacklist
			for (int i = artistBlacklist.Count - 1; i >= 0; i--) {
				if (artistBlacklist[i] == "") {
					artistBlacklist.RemoveAt(i);
				}
			}

			CompositionTarget.Rendering += loop;

		}

		private string prevArtist = "";
		private string currentArtist = "";
		private string prevTrackName = "";

		// Gets called every frame
		private void loop(object sender, EventArgs e) {

			// Obtain information about the track
			string trackInfo = getSpotifyTrackInfo();

			if (trackInfo == "Spotify is not running!" || trackInfo == "Paused") {
				return;
			}

			// Extract the artist and track name
			string artist = "";
			string trackName = "";
			for (int i = 0; i < trackInfo.Length; i++) {
				char cur = trackInfo[i];
				if (cur == '-') {
					if (i != 0 && i != trackInfo.Length - 2) {
						char prev = trackInfo[i - 1];
						char next = trackInfo[i + 1];
						if (prev == ' ' && next == ' ') {
							artist = trackInfo.Substring(0, i - 1);
							trackName = trackInfo.Substring(i + 2);
							break;
						}
					}
				}
			}


			// If no data was extracted, that means its an ad, or paused
			if (artist == "" || artistBlacklist.Contains(artist)) {
				if (trackInfo == "Spotify Free") {
					artist = "";
					trackName = "Paused";
				} else {
					artist = "";
					trackName = "Advertisement";
				}

			}

			// Update labels
			songLbl.Content = trackName;
			artistLbl.Content = artist;

			currentArtist = artist;

			if (artist != prevArtist || trackName != prevTrackName) {
				infoChange(artist, prevArtist, trackName, prevTrackName);
			}

			prevArtist = artist;
			prevTrackName = trackName;

		}

		// Run whenever the artist listening to changes
		private void infoChange(string newArtist, string prevArtist, string newTrack, string prevTrack) {

			if (newTrack.Equals("Spotify") || newTrack.Equals("Advertisement") || artistBlacklist.Contains(newArtist)) {
				// If the artist is Spotify or Advertisement
				var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
				if (proc != null) {
					// Mute spotify
					AudioManager.SetApplicationMute(proc.Id, true);

					// Send notification
					sendNotification("SpotifyMuter", "An ad has started, so your audio was muted.", "");
				}

			} else {
				// Not an ad
				var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
				if (proc != null) {
					// Unmute Spotify
					Task.Delay(500).ContinueWith(_ => {
						AudioManager.SetApplicationMute(proc.Id, false);
					});
				}

				// Send notifications
				if (newTrack.Equals("Paused")) {
					sendNotification("SpotifyMuter", "Your music was paused.", "");
				} else if (prevTrack.Equals("Spotify") || prevTrack.Equals("Advertisement") || artistBlacklist.Contains(prevArtist)) {
					sendNotification("SpotifyMuter", "The ads have ended, enjoy your music!", "");
				}

			}
		}


		private string getSpotifyTrackInfo() {

			var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
			// Check that spotify is open
			if (proc == null) {
				return "Spotify is not running!";
			}

			// The main title contains info about the current track, or ad info
			return proc.MainWindowTitle;
		}

		private void registerAd(object sender, RoutedEventArgs e) {
			if (!artistBlacklist.Contains(currentArtist) && currentArtist != "") {
				artistBlacklist.Add(currentArtist);
				infoChange(currentArtist, prevArtist, "", "");
			}
		}

		private void sendNotification(string title, string line1, string line2) {

			XmlDocument toastXml = new XmlDocument();
			toastXml.LoadXml("<toast duration=\"short\"><visual><binding template=\"ToastText04\"><text id=\"1\">" + title + "</text><text id=\"2\">" + line1 + "</text><text id=\"3\">" + line2 + "</text></binding></visual><audio silent=\"true\"/></toast>");

			XmlElement audio = toastXml.CreateElement("audio");
			audio.SetAttribute("silent", "true");

			ToastNotification toast = new ToastNotification(toastXml);

			ToastNotifier tn = ToastNotificationManager.CreateToastNotifier("JacobHofer.SpotifyMuter");
			tn.Show(toast);

		}

		public void windowClosing(object sender, EventArgs e) {
			// Unmute spotify when we close
			var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
			if (proc != null) {
				AudioManager.SetApplicationMute(proc.Id, false);
			}

			try {
				System.IO.File.WriteAllLines("blacklist.txt", artistBlacklist.ToArray());
			} catch (Exception ex) {
				System.IO.File.Create("blacklist.txt");
			} finally {
				System.IO.File.WriteAllLines("blacklist.txt", artistBlacklist.ToArray());
			}



		}


	}

	[ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(INotificationActivationCallback))]
	[Guid("9A08AE3E-99EF-4AC6-B764-38FCF6BB68A0"), ComVisible(true)]
	public class MyNotificationActivator : NotificationActivator {
		public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId) {
		
		}
	}
}
