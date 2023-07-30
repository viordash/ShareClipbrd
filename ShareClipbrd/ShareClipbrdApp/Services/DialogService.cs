using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {

    public class DialogService : IDialogService {
        public Task ShowMessage(string message) {
            return Dispatcher.UIThread.InvokeAsync(new Func<Task<ButtonResult>>(async () => {
                var msgbox = MessageBoxManager
                            .GetMessageBoxStandard(string.Empty, message, ButtonEnum.Ok, Icon.Info);
                if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    return await msgbox.ShowWindowDialogAsync(desktop.MainWindow);
                }
                return await msgbox.ShowWindowAsync();
            }));
        }

        public Task ShowError(Exception exception) {
            return Dispatcher.UIThread.InvokeAsync(new Func<Task<ButtonResult>>(async () => {
                var msgbox = MessageBoxManager
                            .GetMessageBoxStandard(
                    new MessageBoxStandardParams {
                        ButtonDefinitions = ButtonEnum.Ok,
                        ContentTitle = string.Empty,
                        //ContentHeader = header,
                        ContentMessage = exception.GetBaseException().Message,
                        Icon = Icon.Error,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        ShowInCenter = true,
                        Topmost = true,
                        SystemDecorations = SystemDecorations.Full
                    });
                return await msgbox.ShowWindowAsync();
            }));
        }


        public Task<ShareClipbrd.Core.MessageBoxResult> Confirmation(string messageBoxText, string caption, ShareClipbrd.Core.MessageBoxButton button) {
            var _button = button switch {
                ShareClipbrd.Core.MessageBoxButton.OKCancel => ButtonEnum.OkCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNoCancel => ButtonEnum.YesNoCancel,
                ShareClipbrd.Core.MessageBoxButton.YesNo => ButtonEnum.YesNo,
                _ => ButtonEnum.Ok,
            };

            return Dispatcher.UIThread.InvokeAsync(new Func<Task<ShareClipbrd.Core.MessageBoxResult>>(async () => {
                ButtonResult buttonResult;
                var msgbox = MessageBoxManager
                        .GetMessageBoxStandard(string.Empty, messageBoxText, ButtonEnum.Ok, Icon.Error);
                if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    buttonResult = await msgbox.ShowWindowDialogAsync(desktop.MainWindow);
                } else {
                    buttonResult = await msgbox.ShowWindowAsync();
                }
                return buttonResult switch {
                    ButtonResult.Yes => ShareClipbrd.Core.MessageBoxResult.Yes,
                    ButtonResult.No => ShareClipbrd.Core.MessageBoxResult.No,
                    ButtonResult.Ok => ShareClipbrd.Core.MessageBoxResult.OK,
                    ButtonResult.Cancel => ShareClipbrd.Core.MessageBoxResult.Cancel,
                    _ => ShareClipbrd.Core.MessageBoxResult.None,
                };
            }));
        }
    }
}
