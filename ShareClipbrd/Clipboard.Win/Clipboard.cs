using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clipboard.Core;

namespace Clipboard.Win {
    internal class Clipboard : IClipboard {
        public Task<bool> ContainsFileDropList() {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsImage() {
            throw new NotImplementedException();
        }

        public Task<object?> GetData(string format) {
            throw new NotImplementedException();
        }

        public Task<string[]> GetFormatsAsync() {
            throw new NotImplementedException();
        }
    }
}
