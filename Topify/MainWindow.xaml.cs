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

        private void NextSong_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Forward");
        }
    }
}
