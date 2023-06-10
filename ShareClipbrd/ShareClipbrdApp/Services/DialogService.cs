using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using ShareClipbrd.Core.Services;
using static ShareClipbrd.Core.Clipboard.ClipboardData;

namespace ShareClipbrdApp.Services {

    public class DialogService : IDialogService {
        public Task ShowMessage(string message) {
            return Dispatcher.UIThread.InvokeAsync(new Func<Task<ButtonResult>>(async () => {
                return await MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(string.Empty, message, MessageBox.Avalonia.Enums.ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info)
                        .Show();
            }));
        }

        public Task ShowError(Exception exception) {
            return Dispatcher.UIThread.InvokeAsync(new Func<Task<ButtonResult>>(async () => {
                return await MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow(string.Empty, exception.GetBaseException().Message, MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                                    MessageBox.Avalonia.Enums.Icon.Error)
                            .Show();
            }));
        }


        public Task<ShareClipbrd.Core.MessageBoxResult> Confirmation(string messageBoxText, string caption, ShareClipbrd.Core.MessageBoxButton button) {
            var _button = button switch {
                ShareClipbrd.Core.MessageBoxButton.OKCancel => MessageBox.Avalonia.Enums.ButtonEnum.OkCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNoCancel => MessageBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNo => MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                _ => MessageBox.Avalonia.Enums.ButtonEnum.Ok,
            };

            return Dispatcher.UIThread.InvokeAsync(new Func<Task<ShareClipbrd.Core.MessageBoxResult>>(async () => {
                return await MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(caption, messageBoxText, _button,
                                MessageBox.Avalonia.Enums.Icon.Question)
                        .Show() switch {
                            MessageBox.Avalonia.Enums.ButtonResult.Yes => ShareClipbrd.Core.MessageBoxResult.Yes,
                            MessageBox.Avalonia.Enums.ButtonResult.No => ShareClipbrd.Core.MessageBoxResult.No,
                            MessageBox.Avalonia.Enums.ButtonResult.Ok => ShareClipbrd.Core.MessageBoxResult.OK,
                            MessageBox.Avalonia.Enums.ButtonResult.Cancel => ShareClipbrd.Core.MessageBoxResult.Cancel,
                            _ => ShareClipbrd.Core.MessageBoxResult.None,
                        };
            }));
        }
    }
}
