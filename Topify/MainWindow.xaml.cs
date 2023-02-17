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
using SpotifyAPI;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Newtonsoft.Json;
using static SpotifyAPI.Web.Scopes;

namespace Topify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpotifyClient? spClient;
        private static readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
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
            GetClientOuath();
        }
        public async void GetClientOuath()
        {
            await StartAuthentication();
        }
        private static string? json;
        private static async Task StartAuthentication()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            await _server.Start();
            _server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await _server.Stop();
                PKCETokenResponse token = await new OAuthClient().RequestToken(
                  new PKCETokenRequest("2aa7adaf3dd745d2a5da9ebb12588afe", response.Code, _server.BaseUri, verifier)
                );

                json = JsonConvert.SerializeObject(token);
                await Start();
            };

            var request = new LoginRequest(_server.BaseUri, "2aa7adaf3dd745d2a5da9ebb12588afe", LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string> { UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative }
            };

            Uri uri = request.ToUri();
            try
            {
                BrowserUtil.Open(uri);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to open URL, manually open: {0}", uri);
            }
        }
        private static async Task Start()
        {
            var _token = JsonConvert.DeserializeObject<PKCETokenResponse>(json!);
            var authenticator = new PKCEAuthenticator("2aa7adaf3dd745d2a5da9ebb12588afe", _token!);
            authenticator.TokenRefreshed += (sender, token) => token = _token!;
            var config = SpotifyClientConfig.CreateDefault()
        .WithAuthenticator(authenticator);

            var spotify = new SpotifyClient(config);

            var me = await spotify.UserProfile.Current();
            MessageBox.Show($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");

            var playlists = await spotify.PaginateAll(await spotify.Playlists.CurrentUsers().ConfigureAwait(false));
            MessageBox.Show($"Total Playlists in your Account: {playlists.Count}");

            _server.Dispose();
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
            await spClient!.Player.SkipNext();
            var pb = await spClient.Player.GetCurrentPlayback();
            MessageBox.Show(pb.IsPlaying.ToString());
        }
    }
}
