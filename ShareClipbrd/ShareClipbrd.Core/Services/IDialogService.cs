namespace ShareClipbrd.Core.Services {
    public interface IDialogService {
        Task ShowMessage(string message);
        Task ShowError(Exception exception);
        Task<MessageBoxResult> Confirmation(string messageBoxText, string caption, MessageBoxButton button);
    }
}
