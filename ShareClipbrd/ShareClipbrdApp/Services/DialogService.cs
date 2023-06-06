using System;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {

    public class DialogService : IDialogService {
        public void ShowMessage(string message) {
            MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(string.Empty, message, MessageBox.Avalonia.Enums.ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info)
                        .Show();
        }

        public void ShowError(Exception exception) {
            MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(string.Empty, exception.GetBaseException().Message, MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                                MessageBox.Avalonia.Enums.Icon.Error)
                        .Show();
        }


        public ShareClipbrd.Core.MessageBoxResult Confirmation(string messageBoxText, string caption, ShareClipbrd.Core.MessageBoxButton button) {

            var _button = button switch {
                ShareClipbrd.Core.MessageBoxButton.OKCancel => MessageBox.Avalonia.Enums.ButtonEnum.OkCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNoCancel => MessageBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNo => MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                _ => MessageBox.Avalonia.Enums.ButtonEnum.Ok,
            };

            return MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(caption, messageBoxText, _button,
                                MessageBox.Avalonia.Enums.Icon.Question)
                        .Show().Result switch {
                            MessageBox.Avalonia.Enums.ButtonResult.Yes => ShareClipbrd.Core.MessageBoxResult.Yes,
                            MessageBox.Avalonia.Enums.ButtonResult.No => ShareClipbrd.Core.MessageBoxResult.No,
                            MessageBox.Avalonia.Enums.ButtonResult.Ok => ShareClipbrd.Core.MessageBoxResult.OK,
                            MessageBox.Avalonia.Enums.ButtonResult.Cancel => ShareClipbrd.Core.MessageBoxResult.Cancel,
                            _ => ShareClipbrd.Core.MessageBoxResult.None,
                        };
        }
    }
}
