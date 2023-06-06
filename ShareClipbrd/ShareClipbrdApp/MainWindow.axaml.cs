using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using ShareClipbrdApp.Helpers;

namespace ShareClipbrdApp {
    public partial class MainWindow : Window {
        //bool mouseDownForWindowMoving = false;
        PointerPoint? originalPoint;


        public MainWindow() {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        void OnInitialized(object sender, System.EventArgs e) {
            //WindowsHelper.LoadLocation(Settings.Default.MainFormLocation, this);
            Height = Screens.Primary.WorkingArea.Height / 40;
            Width = Screens.Primary.WorkingArea.Width / 40;
            
            //dataServer.Start();
            //edHostAddress.Text = Settings.Default.HostAddress;
            //edPartnerAddress.Text = Settings.Default.PartnerAddress;
        }

        void OnClosed(object sender, System.EventArgs e) {
            //dataServer.Stop();
            //Settings.Default.MainFormLocation = new System.Drawing.Point((int)Left, (int)Top);
            //Settings.Default.Save();
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
    }
}