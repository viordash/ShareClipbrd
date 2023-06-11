using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Services {
    public class TetrisProgressBar : IProgressBarService {
        private int ordersCount;
        private int ordersSize;
        private Random random;
        private List<int>[] orders = null!;

        private int maxStep { get => ordersCount * ordersSize + ordersCount - 1; }

        private static readonly Color PIXEL_LIGHT = Color.FromRGB(0, 255, 0);
        private static readonly Color PIXEL_DARK = Color.FromRGB(0, 0, 0);

        public TetrisProgressBar(int ordersCount, int ordersSize, int randomSeed) {
            this.ordersCount = ordersCount;
            this.ordersSize = ordersSize;
            this.random = new Random(randomSeed);
            orders = new List<int>[ordersCount];
            RegenerateOrders();
        }

        public void RegenerateOrders() {
            var genRandomArray = (int _) => RandomArrayHelper.GenerateRandomArray(ordersSize, random).ToList();
            orders = Enumerable.Range(0, ordersCount).Select(genRandomArray).ToArray();
        }

        public void UpdateSeed(int newSeed) {
            this.random = new Random(newSeed);
            RegenerateOrders();
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
            var width = progressbarDrawer.Width;
            var height = progressbarDrawer.Height;

            var fStep = (float)step;
            var fWidth = (float)width;
            var fHeight = (float)height;

            progressbarDrawer.Fill(PIXEL_LIGHT);

            var correctXCoord = (int x) => reverse ? width - x - 1 : x;

            var col = Math.Max(fWidth - Math.Ceiling(fStep / fHeight), 0.0);

            Parallel.For(0, height, y => {
                for(int x = width - 1; x >= col; x--) {
                    var order = orders[x];
                    int offset = step - (width - x - 1) * height - order[y];
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
