namespace ShareClipbrd.Core.Services {
    public interface IDialogService {
        void ShowMessage(string message);
        void ShowError(Exception exception);
        MessageBoxResult Confirmation(string messageBoxText, string caption, MessageBoxButton button);
    }
}
