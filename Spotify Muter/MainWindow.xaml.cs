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

			CompositionTarget.Rendering += loop;
		}

		private string prevArtist = "";
		private string currentArtist = "";

		// Gets called every frame
		private void loop(object sender, EventArgs e) {

			// Obtain information about the track
			string trackInfo = GetSpotifyTrackInfo();

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
						}
					}
				}
			}


			// If no data was extracted, that means its an ad
			if (artist == "") {
				artist = "Advertisement";
			}

			currentArtist = artist;

			// Update labels and call artist changed
			songLbl.Content = trackName;
			artistLbl.Content = artist;

			if (artist != prevArtist) {
				artistChange(artist, prevArtist);
			}

			prevArtist = artist;

		}

		// Run whenever the artist listening to changes
		private void artistChange(string newArtist, string prevArtist) {

			if (newArtist.Equals("Spotify") || newArtist.Equals("Advertisement") || artistBlacklist.Contains(newArtist)) {
				// If the artist is Spotify or Advertisement
				var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
				if (proc != null) {
					// Mute spotify
					AudioManager.SetApplicationMute(proc.Id, true);
				}

			} else {
				// Not an ad
				var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
				if (proc != null) {
					// Unmute Spotify
					AudioManager.SetApplicationMute(proc.Id, false);
				}
			}
		}


		private string GetSpotifyTrackInfo() {

			var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
			// Check that spotify is open
			if (proc == null) {
				return "Spotify is not running!";
			}

			// The main title contains info about the current track, or ad info
			return proc.MainWindowTitle;
		}

		private void RegisterAd(object sender, RoutedEventArgs e) {
			if (!artistBlacklist.Contains(currentArtist)) {
				artistBlacklist.Add(currentArtist);
			}
		}

		public void WindowClosing(object sender, EventArgs e) {
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
}
