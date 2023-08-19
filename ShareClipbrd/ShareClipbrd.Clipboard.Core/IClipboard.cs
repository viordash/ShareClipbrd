using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareClipbrd.Clipboard.Core {
    internal interface IClipboard {
        Task<string[]> GetFormatsAsync();
        Task<bool> ContainsFileDropList();
        Task<bool> ContainsImage();

        Task<object?> GetData(string format);
    }
}
