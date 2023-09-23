using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Clipboard;
using Clipboard.Core;
using GuardNet;
using ShareClipbrd.Core.Configuration;
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
        readonly IProgressService? progressService;
        readonly ISystemConfiguration? systemConfiguration;

        WriteableBitmap? progressBarBitmap;
        public WriteableBitmap? ProgressBarBitmap {
            get => progressBarBitmap;
            set => progressBarBitmap = value;
        }

        readonly TetrisProgressBar? progressBar;

        public MainWindow(
            IDataClient dataClient,
            IDataServer dataServer,
            IDialogService dialogService,
            IProgressService progressService,
            ISystemConfiguration? systemConfiguration) : this() {
            Guard.NotNull(dataClient, nameof(dataClient));
            Guard.NotNull(dataServer, nameof(dataServer));
            Guard.NotNull(dialogService, nameof(dialogService));
            Guard.NotNull(progressService, nameof(progressService));
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));

            progressBar = new TetrisProgressBar((int)Width - 6, (int)Height - 6, new Random().Next());
            progressBarBitmap = new WriteableBitmap(new PixelSize(progressBar.Width, progressBar.Height), new Vector(1.0, 1.0),
                            Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);


            SuperImage.Source = progressBarBitmap;

            this.dataClient = dataClient;
            this.dataServer = dataServer;
            this.dialogService = dialogService;
            this.progressService = progressService;
            this.systemConfiguration = systemConfiguration;
            SetProgressMode(ProgressMode.None);
            SetProgress(100.0);
        }

        public MainWindow() {
            InitializeComponent();
        }

        void OnOpened(object sender, System.EventArgs e) {
            WindowsHelper.LoadLocation(Settings.Default.MainFormLocation, this);
            edSettingsProfile.SelectedIndex = systemConfiguration!.SettingsProfile;
            edHostAddress.Text = systemConfiguration!.HostAddress;
            edPartnerAddress.Text = systemConfiguration!.PartnerAddress;

        }

        void OnClosing(object sender, WindowClosingEventArgs e) {
            dataClient?.Stop();
            dataServer?.Stop();
            Settings.Default.MainFormLocation = new System.Drawing.Point(Position.X, Position.Y);
            Settings.Default.Save();
        }

        void OnActivated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.Aqua;
            Opacity = 1;
        }

        void OnDeactivated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.DarkGoldenrod;
            Opacity = 0.6;
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

        void MenuItemExit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void edHostAddress_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if(e.Property == TextBox.TextProperty) {
                switch(systemConfiguration!.SettingsProfile) {
                    case 0:
                        Settings.Default.HostAddress0 = edHostAddress.Text;
                        break;
                    case 1:
                        Settings.Default.HostAddress1 = edHostAddress.Text;
                        break;
                    case 2:
                        Settings.Default.HostAddress2 = edHostAddress.Text;
                        break;
                }
            }
        }

        private void edPartnerAddress_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if(e.Property == TextBox.TextProperty) {
                switch(systemConfiguration!.SettingsProfile) {
                    case 0:
                        Settings.Default.PartnerAddress0 = edPartnerAddress.Text;
                        break;
                    case 1:
                        Settings.Default.PartnerAddress1 = edPartnerAddress.Text;
                        break;
                    case 2:
                        Settings.Default.PartnerAddress2 = edPartnerAddress.Text;
                        break;
                }
            }
        }

        private void edSettingsProfile_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
            Settings.Default.SettingsProfile = edSettingsProfile.SelectedIndex;
            SettingsUpdated();
        }

        async void SettingsUpdated() {
            edHostAddress.Text = systemConfiguration!.HostAddress;
            edPartnerAddress.Text = systemConfiguration!.PartnerAddress;
            await (dataServer?.Stop() ?? Task.CompletedTask);
            dataServer?.Start();
            dataClient?.Stop();
            dataClient?.Start();
        }

        void OnKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control) {
                TransmitClipboard();
            }
        }

        public async void TransmitClipboard() {
            try {
                var clipboard = ClipboardProvider.Get(this);

                if(await clipboard.ContainsFileDropList()) {
                    Debug.WriteLine("ContainsFileDropList");

                    var formats = await clipboard.GetFormats();
                    var fileDropList = await ClipboardFile.GetList(formats, async (format) => {
                        var filesData = await clipboard.GetData(format);
                        return filesData;
                    });
                    await dataClient!.SendFileDropList(fileDropList);
                } else {
                    var formats = await clipboard.GetFormats();
                    var clipboardData = new ClipboardData();
                    await clipboardData.Serialize(formats, clipboard.GetData);
                    if(clipboardData.Formats.Any()) {
                        await dataClient!.SendData(clipboardData);
                        return;
                    }
                }

                await using(progressService!.Begin(ProgressMode.Error)) {
                    Debug.WriteLine("Data empty error");
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
            // Debug.WriteLine($"{percent:0.#####}");
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

        public void ShowConnectStatus(bool online) {
            crOnline.IsVisible = online;
        }

        public void ShowClientConnectStatus(bool online) {
            crClientOnline.IsVisible = online;
        }
    }
}
