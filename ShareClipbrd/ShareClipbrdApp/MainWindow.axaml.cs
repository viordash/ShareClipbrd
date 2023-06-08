using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GuardNet;
using ShareClipbrd.Core.Clipboard;
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
            Border.Opacity = 1;
        }

        void OnDeactivated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.DarkGoldenrod;
            Border.Opacity = 0.5;
        }

        void OnPointerPressedEvent(object? sender, PointerPressedEventArgs e) {
            if(e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                originalPoint = e.GetCurrentPoint(this);
            }
        }

        private void OnPointerReleasedEvent(object? sender, PointerReleasedEventArgs e) {
            originalPoint = null;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e) {
            if(!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                return;
            }
            if(originalPoint == null) {
                return;
            }

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - originalPoint.Position.X),
                Position.Y + (int)(currentPoint.Position.Y - originalPoint.Position.Y));
        }

        void MenuItemPaste_Click(object sender, RoutedEventArgs e) {
            TransmitClipboard();
        }

        void MenuItemClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void edHostAddress_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if(e.Property == TextBox.TextProperty) {
                Settings.Default.HostAddress = edHostAddress.Text;
            }
        }

        private void edPartnerAddress_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if(e.Property == TextBox.TextProperty) {
                Settings.Default.PartnerAddress = edPartnerAddress.Text;
            }
        }

        void OnKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control) {
                TransmitClipboard();
            }
        }

        async void TransmitClipboard() {
            var clipboardData = new ClipboardData();
            //if(System.Windows.Clipboard.ContainsFileDropList()) {
            //    var fileDropList = System.Windows.Clipboard.GetFileDropList();
            //    _ = Task.Run(async () => await dataClient.SendFileDropList(fileDropList));
            //} else if(System.Windows.Clipboard.ContainsImage()) {

            //} else if(System.Windows.Clipboard.ContainsAudio()) {

            //} else {
            var formats = await Application.Current!.Clipboard!.GetFormatsAsync();
            clipboardData.Serialize(formats, Application.Current.Clipboard.GetDataAsync);

            await dataClient!.SendData(clipboardData);
            //}
        }

        public void SetProgress(double percent) {
            pbOperation.Width = Width * Math.Clamp(percent, 0.0, 100.0) / 100.0;
        }

        public void SetProgressMinor(double percent) {
            pbOperationMinor.Width = Width * Math.Clamp(percent, 0.0, 100.0) / 100.0;
        }

        public void SetProgressMode(ProgressMode mode) {
            switch(mode) {
                case ProgressMode.Send:
                    Border.Background = new SolidColorBrush(Colors.GreenYellow);
                    break;
                case ProgressMode.Receive:
                    Border.Background = new SolidColorBrush(Colors.LightYellow);
                    break;
                case ProgressMode.Error:
                    Border.Background = new SolidColorBrush(Colors.IndianRed);
                    break;
                default:
                    Border.Background = new SolidColorBrush(Colors.LightSteelBlue);
                    break;

            }
        }
    }
}