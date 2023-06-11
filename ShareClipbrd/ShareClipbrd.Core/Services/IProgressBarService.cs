using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareClipbrd.Core.Services {
	public interface IProgressBarService {
		public void SetProgress(double progress, RawBitmapDrawer progressBarBitmap);
		public void SetProgressStepped(int step, RawBitmapDrawer progressBarBitmap) { }
	}
}
