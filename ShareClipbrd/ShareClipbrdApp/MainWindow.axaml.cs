using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
        readonly IDialogService? dialogService;

        WriteableBitmap? progressBarBitmap;
        public WriteableBitmap? ProgressBarBitmap {
            get => progressBarBitmap;
            set => progressBarBitmap = value;
        }

        readonly TetrisProgressBar? progressBar;

        // for tests
        double progress = 0;
        private void test(Object source, ElapsedEventArgs e) {
            if(progress >= 100.0) {
                progress *= -1.0;
            }
            progress += 0.5;
            Dispatcher.UIThread.InvokeAsync(() => SetProgress(progress), DispatcherPriority.MaxValue);
        }

        public MainWindow(
            IDataClient dataClient,
            IDataServer dataServer,
            IDialogService dialogService) : this() {
            Guard.NotNull(dataClient, nameof(dataClient));
            Guard.NotNull(dataServer, nameof(dataServer));
            Guard.NotNull(dialogService, nameof(dialogService));

            progressBar = new TetrisProgressBar((int)Width - 6, (int)Height - 6, new Random().Next());
            progressBarBitmap = new WriteableBitmap(new PixelSize(progressBar.Width, progressBar.Height), new Vector(1.0, 1.0),
                            Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);
            //SetProgress(progress);

            SuperImage.Source = progressBarBitmap;

            this.dataClient = dataClient;
            this.dataServer = dataServer;
            this.dialogService = dialogService;

            // for tests
            var redrawTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(10));
            redrawTimer.Elapsed += test;
            redrawTimer.Enabled = true;
            redrawTimer.AutoReset = true;
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
            Border.Opacity = 0.7;
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
            try {
                var formats = await Application.Current!.Clipboard!.GetFormatsAsync();
                var clipboardData = new ClipboardData();
                if(clipboardData.ContainsFileDropList(formats)) {
                    var fileDropList = await clipboardData.GetFileDropList(Application.Current.Clipboard.GetDataAsync);
                    await dataClient!.SendFileDropList(fileDropList);
                    //} else if(System.Windows.Clipboard.ContainsImage()) {

                    //} else if(System.Windows.Clipboard.ContainsAudio()) {

                } else {
                    await clipboardData.Serialize(formats, Application.Current.Clipboard.GetDataAsync);
                    await dataClient!.SendData(clipboardData);
                }
            } catch(SocketException ex) {
                await dialogService!.ShowError(ex);
            } catch(InvalidDataException ex) {
                await dialogService!.ShowError(ex);
            } catch(IOException ex) {
                await dialogService!.ShowError(ex);
            }
        }

        public void SetProgress(double percent) {
            //pbOperation.Width = Width * Math.Clamp(percent, 0.0, 100.0) / 100.0;
            SetProgressMinor(percent);
            using(var pixelsLock = progressBarBitmap!.Lock()) unsafe {
                    var rawBitmapDrawer = new RawBitmapDrawer(progressBar!.Width, progressBar!.Height, pixelsLock.Address);
                    progressBar.SetProgress(percent, rawBitmapDrawer);
                }
            SuperImage.InvalidateVisual();
        }

        public void SetProgressMinor(double percent) {
            //pbOperationMinor.Width = Width * Math.Clamp(percent, 0.0, 100.0) / 100.0;
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
                    Border.Background = new SolidColorBrush(Colors.PowderBlue);
                    break;

            }
        }
    }
}
