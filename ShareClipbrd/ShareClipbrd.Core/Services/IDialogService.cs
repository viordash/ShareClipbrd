namespace ShareClipbrd.Core.Services {
    public interface IDialogService {
        string? OpenFileDialog(string filter);
        string? SaveFileDialog(string filter);
        void ShowMessage(string message);
        void ShowError(Exception exception);
        MessageBoxResult Confirmation(string messageBoxText, string caption, MessageBoxButton button);
    }
}
