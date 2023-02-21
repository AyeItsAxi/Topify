using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Topify.Common;
using Newtonsoft.Json;
using SpotifyApi.NetCore.Authorization;
using SpotifyApi.NetCore;
using System.Diagnostics;
using System.Net.Http;

namespace Topify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWindowSettings();
        }
        public void InitializeWindowSettings()
        {
            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();
            var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            DragContainer.Visibility = Visibility.Visible;
            Environment.SetEnvironmentVariable("SpotifyApiClientId", "2aa7adaf3dd745d2a5da9ebb12588afe");
            Environment.SetEnvironmentVariable("SpotifyApiClientSecret", "29588318688e4e838fd0950267d2cbd8");
            Authify();
        }
        private async void Authify()
        {
            var http = new HttpClient();
            var d = new AccountsService(http);
            var song = new TracksApi(http, d);
            Track tr = await song.GetTrack("1tpXaFf2F55E7kVJON4j4G");
            string name = tr.Album.ToString();
            MessageBox.Show(name);
            //var accounts = new UserAccountsService((Microsoft.Extensions.Configuration.IConfiguration)http);
            /*
            // See https://developer.spotify.com/documentation/general/guides/authorization-guide/#authorization-code-flow
            //  for an explanation of the Authorization code flow

            // Generate a random state value to use in the Auth request
            string state = Guid.NewGuid().ToString("N");
            // Accounts service will derive the Auth URL for you
            string url = accounts.AuthorizeUrl(state, new[] { "user-read-playback-state" });

            /*
                Redirect the user to `url` and when they have auth'ed Spotify will redirect to your reply URL
                The response will include two query parameters: `state` and `code`.
                For a full working example see `SpotifyApi.NetCore.Samples`.
            */

            // Check that the request has not been tampered with by checking the `state` value matches
            /*if (state != query["state"]) throw new ArgumentException();

            // Use the User accounts service to swap `code` for a Refresh token
            BearerAccessRefreshToken token = await accounts.RequestAccessRefreshToken(query["code"]);

            // Use the Bearer (Access) Token to call the Player API
            var player = new PlayerApi(http, accounts);
            Device[] devices = await player.GetDevices(accessToken: token.AccessToken);

            foreach (Device device in devices)
            {
                Trace.WriteLine($"Device {device.Name} Status = {device.Type} Active = {device.IsActive}");
            }*/
        }
        // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
        // Copied from dwmapi.h
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);

        private void DragContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void PreviousSong_MouseEnter(object sender, MouseEventArgs e)
        {
            SkipBackGeometry.Brush = Brushes.White;
            AnimationHandler.FadeAnimation(SkipBackDrawingImage, 0.2, SkipBackDrawingImage.Opacity, 1);
        }

        private async void PreviousSong_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationHandler.FadeAnimation(SkipBackDrawingImage, 0.2, SkipBackDrawingImage.Opacity, 0.3);
            await Task.Delay(225);
            SkipBackGeometry.Brush = Brushes.Black;
        }

        private void PreviousSong_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Back");
        }

        private void NextSong_MouseEnter(object sender, MouseEventArgs e)
        {
            SkipForwardGeometry.Brush = Brushes.White;
            AnimationHandler.FadeAnimation(SkipForwardDrawingImage, 0.2, SkipForwardDrawingImage.Opacity, 1);
        }

        private async void NextSong_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationHandler.FadeAnimation(SkipForwardDrawingImage, 0.2, SkipForwardDrawingImage.Opacity, 0.3);
            await Task.Delay(225);
            SkipForwardGeometry.Brush = Brushes.Black;
        }

        private async void NextSong_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //await spClient!.Player.SkipNext();
            //var pb = await spClient.Player.GetCurrentPlayback();
            //MessageBox.Show(pb.IsPlaying.ToString());
        }
    }
}
