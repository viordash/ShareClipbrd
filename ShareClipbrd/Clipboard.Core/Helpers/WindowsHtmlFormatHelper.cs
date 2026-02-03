using System.Text;
using System.Text.RegularExpressions;

namespace Clipboard.Core.Helpers {
    /// <summary>
    /// Helper for parsing Windows HTML Clipboard Format.
    /// Format specification: https://docs.microsoft.com/en-us/windows/win32/dataxchg/html-clipboard-format
    /// </summary>
    public static class WindowsHtmlFormatHelper {
        static readonly Regex startFragmentRegex = new(@"StartFragment:(\d+)", RegexOptions.Compiled);
        static readonly Regex endFragmentRegex = new(@"EndFragment:(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the HTML fragment from Windows HTML Clipboard Format.
        /// </summary>
        /// <param name="windowsHtmlFormat">Raw Windows HTML Format data</param>
        /// <returns>Clean HTML fragment, or original data if parsing fails</returns>
        public static byte[] ExtractHtmlFragment(byte[] windowsHtmlFormat) {
            if(windowsHtmlFormat is null) {
                return [];
            }
            if(!windowsHtmlFormat.Any()) {
                return windowsHtmlFormat;
            }

            try {
                var text = Encoding.UTF8.GetString(windowsHtmlFormat);

                var startMatch = startFragmentRegex.Match(text);
                var endMatch = endFragmentRegex.Match(text);

                if(!startMatch.Success || !endMatch.Success) {
                    return windowsHtmlFormat;
                }

                var startFragment = int.Parse(startMatch.Groups[1].Value);
                var endFragment = int.Parse(endMatch.Groups[1].Value);

                if(startFragment < 0 || endFragment <= startFragment || endFragment > windowsHtmlFormat.Length) {
                    return windowsHtmlFormat;
                }

                var fragmentLength = endFragment - startFragment;
                var fragment = new byte[fragmentLength];
                Array.Copy(windowsHtmlFormat, startFragment, fragment, 0, fragmentLength);

                return fragment;
            } catch {
                return windowsHtmlFormat;
            }
        }
    }
}
