using System;
using System.Threading.Tasks;

namespace ShareClipbrdApp.Components {
    public class TetrisProgressBar {
        public int Width { get; private set; }
        public int Height { get; private set; }
        readonly int[,] orders;

        int maxStep { get => Width * Height + Width - 1; }

        static readonly Color PIXEL_LIGHT = Color.FromRGB(0, 255, 0);
        static readonly Color PIXEL_DARK = Color.FromRGB(0, 0, 0);

        public TetrisProgressBar(int width, int height, int randomSeed) {
            Width = width;
            Height = height;
            orders = new int[Width, Height];
            RegenerateOrders(randomSeed);
        }

        public void RegenerateOrders(int randomSeed) {
            var random = new Random(randomSeed);
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    orders[x, y] = y;
                    var rnd = random.Next(0, y);
                    (orders[x, y], orders[x, rnd]) = (orders[x, rnd], orders[x, y]);
                }
            }
        }

        public void UpdateSeed(int newSeed) {
            RegenerateOrders(newSeed);
        }

        public void SetProgress(double progress, RawBitmapDrawer progressbarDrawer) {
            var step = (int)Math.Round((double)maxStep * Math.Clamp(progress, -100.0, 100.0) / 100.0);
            SetProgressStepped(step, progressbarDrawer);
        }

        public void SetProgressStepped(int step, RawBitmapDrawer progressbarDrawer) {
            var reverse = false;
            if(step < 0) {
                step *= -1;
                reverse = true;
            }
            SetProgressReversable(step, progressbarDrawer, reverse);
        }

        private void SetProgressReversable(int step, RawBitmapDrawer progressbarDrawer, bool reverse) {
            var width = Width;
            var height = Height;

            var fStep = (float)step;
            var fWidth = (float)width;
            var fHeight = (float)height;

            progressbarDrawer.Fill(PIXEL_LIGHT);

            var correctXCoord = (int x) => reverse ? width - x - 1 : x;

            var col = Math.Max(fWidth - Math.Ceiling(fStep / fHeight), 0.0);

            Parallel.For(0, height, y => {
                for(int x = width - 1; x >= col; x--) {
                    int offset = step - (width - x - 1) * height - orders[x, y];
                    if(offset > 0) {
                        progressbarDrawer.SetPixel(correctXCoord(x), y, PIXEL_DARK);
                        int xNew = x + offset;
                        if(xNew < width) {
                            progressbarDrawer.SetPixel(correctXCoord(xNew), y, PIXEL_LIGHT);
                        }
                    }
                }
            });
        }
    }
}
