using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GuardNet;
using ShareClipbrd.Core.Services;
using ShareClipbrdApp.Helpers;
using ShareClipbrdApp.Properties;

namespace ShareClipbrdApp {
    public partial class MainWindow : Window {
        PointerPoint? originalPoint;
        readonly IDataClient? dataClient;
        readonly IDataServer? dataServer;

        public MainWindow(
            IDataClient dataClient,
            IDataServer dataServer) : this() {
            Guard.NotNull(dataClient, nameof(dataClient));
            Guard.NotNull(dataServer, nameof(dataServer));

            this.dataClient = dataClient;
            this.dataServer = dataServer;
        }

        public MainWindow() {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        void OnOpened(object sender, System.EventArgs e) {
            WindowsHelper.LoadLocation(Settings.Default.MainFormLocation, this);
            Height = Screens.Primary.WorkingArea.Height / 40;
            Width = Screens.Primary.WorkingArea.Width / 40;

            dataServer?.Start();
            edHostAddress.Text = Settings.Default.HostAddress;
            edPartnerAddress.Text = Settings.Default.PartnerAddress;
        }

        void OnClosing(object sender, CancelEventArgs e) {
            dataServer?.Stop();
            Settings.Default.MainFormLocation = new System.Drawing.Point(Position.X, Position.Y);
            Settings.Default.Save();
        }

        void OnActivated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.Aqua;
            Border.BorderThickness = new Thickness(2);
            Opacity = 1;
        }

        void OnDeactivated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.DarkGoldenrod;
            Border.BorderThickness = new Thickness(1);
            Opacity = 0.345;
        }

        void OnPointerPressedEvent(object? sender, PointerPressedEventArgs e) {
            originalPoint = e.GetCurrentPoint(this);
        }

        private void OnPointerReleasedEvent(object? sender, PointerReleasedEventArgs e) {
            originalPoint = null;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            if(originalPoint == null) {
                return;
            }

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - originalPoint.Position.X),
                Position.Y + (int)(currentPoint.Position.Y - originalPoint.Position.Y));
        }

        void MenuItemPaste_Click(object sender, RoutedEventArgs e) {
            //TransmitClipboard();
            //edHostAddress.cha
        }

        void MenuItemClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void edHostAddress_TextChanged(object sender, TextInputEventArgs e) {
            Settings.Default.HostAddress = edHostAddress.Text;
        }

        private void edPartnerAddress_TextChanged(object sender, TextInputEventArgs e) {
            Settings.Default.PartnerAddress = edPartnerAddress.Text;
        }
    }
}