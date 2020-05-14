using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DesktopNotifications;

namespace Spotify_Muter {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		protected override void OnStartup(StartupEventArgs e) {

			DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("JacobHofer.SpotifyMuter");
			DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();

			base.OnStartup(e);
		}

	}
}
