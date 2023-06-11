namespace ShareClipbrd.Core.Helpers {
    public class RandomArrayHelper {
        public static void ShuffleArray(int[] arr, Random random) {
            for(int idx = 0; idx < arr.Length; idx++) {
                var randomIdx = random.Next(0, idx);
                (arr[idx], arr[randomIdx]) = (arr[randomIdx], arr[idx]);
            }
        }

        public static int[] GenerateRandomArray(int size, Random random) {
            var array = Enumerable.Range(0, size).ToArray();
            ShuffleArray(array, random);
            return array;
        }
    }
}
