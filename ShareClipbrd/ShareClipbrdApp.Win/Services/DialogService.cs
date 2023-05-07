using System;
using Microsoft.Win32;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {

    public class DialogService : IDialogService {

        public string? OpenFileDialog(string filter) {
            var fileDialog = new OpenFileDialog {
                Filter = filter
            };
            if(fileDialog.ShowDialog() == true) {
                return fileDialog.FileName;
            }
            return string.Empty;
        }

        public string? SaveFileDialog(string filter) {
            var fileDialog = new SaveFileDialog {
                Filter = filter
            };
            if(fileDialog.ShowDialog() == true) {
                return fileDialog.FileName;
            }
            return null;
        }

        public void ShowMessage(string message) {
            System.Windows.MessageBox.Show(message, string.Empty, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public void ShowError(Exception exception) {
            System.Windows.MessageBox.Show(exception.GetBaseException().Message, string.Empty, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }


        public ShareClipbrd.Core.MessageBoxResult Confirmation(string messageBoxText, string caption, ShareClipbrd.Core.MessageBoxButton button) {
            var _button = button switch {
                ShareClipbrd.Core.MessageBoxButton.OKCancel => System.Windows.MessageBoxButton.OKCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNoCancel => System.Windows.MessageBoxButton.YesNoCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNo => System.Windows.MessageBoxButton.YesNo,
                _ => System.Windows.MessageBoxButton.OK,
            };

            return System.Windows.MessageBox.Show(messageBoxText, caption, _button, System.Windows.MessageBoxImage.Question) switch {
                System.Windows.MessageBoxResult.Yes => ShareClipbrd.Core.MessageBoxResult.Yes,
                System.Windows.MessageBoxResult.No => ShareClipbrd.Core.MessageBoxResult.No,
                System.Windows.MessageBoxResult.OK => ShareClipbrd.Core.MessageBoxResult.OK,
                System.Windows.MessageBoxResult.Cancel => ShareClipbrd.Core.MessageBoxResult.Cancel,
                _ => ShareClipbrd.Core.MessageBoxResult.None,
            };
        }
    }
}
