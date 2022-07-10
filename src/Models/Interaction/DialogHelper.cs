using M64RPFW.UI.Views.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace M64RPFW.UI.ViewModels.Interaction
{

    // INFO
    // Violation of MVVM. This is an arbitrary imposition by MaterialDesignInXAML and could be mitigated by exposing dialog functions via interface
    public static class DialogHelper
    {
        public static Snackbar CurrentSnackbar { private get; set; }

        public static bool ShowErrorDialogIf(string message, bool condition)
        {
            if (condition) ShowErrorDialog(message);
            return condition;
        }
        public static void ShowErrorDialog(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DialogHost.Show(new ErrorDialog
                {
                    Message = { Text = message },
                }, "RootDialog");
            }));
        }
        public static void ShowSnackbar(string message)
        {
            if (CurrentSnackbar == null) throw new ArgumentNullException("CurrentSnackbar");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Task.Factory.StartNew(() => Thread.Sleep(0)).ContinueWith(t =>
                {
                    CurrentSnackbar.MessageQueue?.Enqueue(message);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }));

        }
    }
}
