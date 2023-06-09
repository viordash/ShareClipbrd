using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GuardNet;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Services;
using ShareClipbrdApp.Components;
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


            SuperImage.Source = progressBarBitmap;

            this.dataClient = dataClient;
            this.dataServer = dataServer;
            this.dialogService = dialogService;

            SetProgressMode(ProgressMode.None);
            SetProgress(100.0);
        }

        public MainWindow() {
            InitializeComponent();
        }

        void OnOpened(object sender, System.EventArgs e) {
            WindowsHelper.LoadLocation(Settings.Default.MainFormLocation, this);

            dataServer?.Start();
            edHostAddress.Text = Settings.Default.HostAddress;
            edPartnerAddress.Text = Settings.Default.PartnerAddress;
        }

        void OnClosing(object sender, WindowClosingEventArgs e) {
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
            Border.Opacity = 0.6;
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
            if(!originalPoint.HasValue) {
                return;
            }

            PointerPoint currentPoint = e.GetCurrentPoint(this);
            Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - originalPoint.Value.Position.X),
                Position.Y + (int)(currentPoint.Position.Y - originalPoint.Value.Position.Y));
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
                var clipboard = GetTopLevel(this)!.Clipboard!;
                var formats = await clipboard.GetFormatsAsync();

                var fileDropList = await ClipboardFile.GetList(formats, clipboard.GetDataAsync);
                if(fileDropList.Count > 0) {
                    await dataClient!.SendFileDropList(fileDropList);
                    return;
                }

                var clipboardData = new ClipboardData();
                await clipboardData.Serialize(formats, clipboard.GetDataAsync);
                await dataClient!.SendData(clipboardData);

            } catch(SocketException ex) {
                await dialogService!.ShowError(ex);
            } catch(InvalidDataException ex) {
                await dialogService!.ShowError(ex);
            } catch(IOException ex) {
                await dialogService!.ShowError(ex);
            } catch(OperationCanceledException) {
            }
        }

        public void SetProgress(double percent) {
            Debug.WriteLine($"{percent:0.#####}");
            using(var pixelsLock = progressBarBitmap!.Lock()) unsafe {
                    var rawBitmapDrawer = new RawBitmapDrawer(progressBar!.Width, progressBar!.Height, pixelsLock.Address);
                    progressBar.SetProgress(percent, rawBitmapDrawer);
                }
            SuperImage.InvalidateVisual();
        }


        public void SetProgressMode(ProgressMode mode) {
            switch(mode) {
                case ProgressMode.Send:
                    Border.Background = new SolidColorBrush(Colors.GreenYellow);
                    SuperImage.IsVisible = true;
                    break;
                case ProgressMode.Receive:
                    Border.Background = new SolidColorBrush(Colors.LightYellow);
                    SuperImage.IsVisible = true;
                    break;
                case ProgressMode.Error:
                    Border.Background = new SolidColorBrush(Colors.IndianRed);
                    SuperImage.IsVisible = false;
                    break;
                default:
                    Border.Background = new SolidColorBrush(Colors.PowderBlue);
                    SuperImage.IsVisible = false;
                    break;
            }
        }
    }
}
