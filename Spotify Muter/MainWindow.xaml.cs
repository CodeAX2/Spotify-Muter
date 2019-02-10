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

		public MainWindow() {

			InitializeComponent();
			Title = "Spotify Muter";
			ResizeMode = ResizeMode.NoResize;

			// Get the label
			songLbl = (Label)this.FindName("Song");
			artistLbl = (Label)this.FindName("Artist");

			CompositionTarget.Rendering += loop;
		}

		private string prevArtist = "";

		private void loop(object sender, EventArgs e) {
			string trackInfo = GetSpotifyTrackInfo();

			if (trackInfo == "Spotify is not running!" || trackInfo == "Paused") {
				return;
			}

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


			if (artist == "") {
				artist = trackInfo;
			}

			songLbl.Content = trackName;
			artistLbl.Content = artist;

			if (artist != prevArtist) {
				artistChange(artist, prevArtist);
			}

			prevArtist = artist;

		}

		private void artistChange(string newArtist, string prevArtist) {

			if (newArtist.Equals("Spotify") || newArtist.Equals("Advertisement")) {

				var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
				if (proc != null) {
					AudioManager.SetApplicationMute(proc.Id, true);
				}

			} else {
				var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
				if (proc != null) {
					AudioManager.SetApplicationMute(proc.Id, false);
				}
			}
		}


		private string GetSpotifyTrackInfo() {
			var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

			if (proc == null) {
				return "Spotify is not running!";
			}

			return proc.MainWindowTitle;
		}


	}
}
