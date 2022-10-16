using System.Windows;

namespace M64RPFW.UI.ViewModels.Interaction
{
    public class StatusInformation
    {
        public bool Success { get; private set; }
        public string? Message { get; private set; }

        public StatusInformation(string? message)
        {
            Success = false;
            Message = message;
        }

        public StatusInformation()
        {
            Success = true;
        }

        public void ShowDialogIfFailed()
        {
            if (!Success)
            {
                MessageBox.Show(Message);
            }
        }
    }
}
