namespace M64RPFW.ViewModels.Interfaces
{
    public interface IFileDialogProvider
    {
        public (string ReturnedPath, bool Cancelled) OpenFileDialogPrompt(string[] validExtensions);
        public (string ReturnedPath, bool Cancelled) SaveFileDialogPrompt(string[] validExtensions);
    }
}
