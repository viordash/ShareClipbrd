using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GuardNet;
using ShareClipbrd.Core;
using ShareClipbrd.Core.Services;
using ShareClipbrdApp.Win.Helpers;
using ShareClipbrdApp.Win.Properties;

namespace ShareClipbrdApp.Win {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public System.Drawing.Rectangle Bounds { get { return new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height); } }

        readonly IDataTransferService dataTransferService;
        readonly IClipboardService clipboardService;

        public MainWindow(
            IDataTransferService dataTransferService,
            IClipboardService clipboardService) {
            Guard.NotNull(dataTransferService, nameof(dataTransferService));
            Guard.NotNull(clipboardService, nameof(clipboardService));

            this.dataTransferService = dataTransferService;
            this.clipboardService = clipboardService;

            InitializeComponent();
        }

        void Window_Initialized(object sender, System.EventArgs e) {
            WindowsHelper.LoadLocation(Settings.Default.MainFormLocation, this);
            Height = SystemParameters.FullPrimaryScreenHeight / 40;
            Width = SystemParameters.FullPrimaryScreenWidth / 40;
        }

        void Window_Closed(object sender, System.EventArgs e) {
            Settings.Default.MainFormLocation = new System.Drawing.Point((int)Left, (int)Top);
            Settings.Default.Save();
        }

        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        void MenuItemClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        async void Window_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control) {
                await TransmitClipboard();
            }
        }

        async void MenuItemPaste_Click(object sender, RoutedEventArgs e) {
            await TransmitClipboard();
        }

        void Window_Activated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.Aqua;
            Border.BorderThickness = new Thickness(2);
        }

        void Window_Deactivated(object sender, System.EventArgs e) {
            Border.BorderBrush = Brushes.DarkGoldenrod;
            Border.BorderThickness = new Thickness(1);
        }

        async Task TransmitClipboard() {
            await using(ProcessIndicator.Indicate(this)) {
                ClipboardData clipboardData;
                if(Clipboard.ContainsFileDropList()) {
                    clipboardData = clipboardService.GetSerializedFiles(Clipboard.GetFileDropList());
                } else if(Clipboard.ContainsImage()) {
                    clipboardData = new();
                } else if(Clipboard.ContainsAudio()) {
                    clipboardData = new();
                } else {
                    var dataObject = Clipboard.GetDataObject();
                    clipboardData = clipboardService.GetSerializedDataObjects(dataObject.GetFormats(), dataObject.GetData);
                }

                await dataTransferService.Send(clipboardData);
            }
        }
    }
}
