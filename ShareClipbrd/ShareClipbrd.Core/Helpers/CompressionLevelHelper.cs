using ShareClipbrd.Core.Configuration;

namespace ShareClipbrd.Core.Helpers {

    public class CompressionLevelHelper {
        public static Dictionary<CompressionLevel, string> Names = new(){
            { CompressionLevel.Optimal, "Optimal" },
            { CompressionLevel.Fastest, "Fastest" },
            { CompressionLevel.NoCompression, "No compression" }
        };

        public static string GetName(int value) =>
           (CompressionLevel)value switch {
               CompressionLevel.Optimal => Names[CompressionLevel.Optimal],
               CompressionLevel.Fastest => Names[CompressionLevel.Fastest],
               _ => Names[CompressionLevel.NoCompression],
           };

        public static CompressionLevel GetValue(string? name) {
            if(name == Names[CompressionLevel.Optimal]) {
                return CompressionLevel.Optimal;
            }
            if(name == Names[CompressionLevel.Fastest]) {
                return CompressionLevel.Fastest;
            }
            return CompressionLevel.NoCompression;
        }

        public static System.IO.Compression.CompressionLevel GetLevel(CompressionLevel level) =>
           level switch {
               CompressionLevel.Optimal => System.IO.Compression.CompressionLevel.Optimal,
               CompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
               _ => System.IO.Compression.CompressionLevel.NoCompression,
           };


    }
}
