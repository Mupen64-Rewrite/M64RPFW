using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Input;

namespace M64RPFW.src.Extensions
{
    /// <summary>
    /// This class facilitates associating a key binding in XAML markup to  a command
    /// defined in a View Model by exposing a Command dependency property.
    /// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
    /// </summary>
    public class CommandReference : Freezable, ICommand
    {
        public CommandReference()
        {
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(RelayCommand), typeof(CommandReference), new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));

        public RelayCommand Command
        {
            get => (RelayCommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        #region RelayCommand Members

        public bool CanExecute(object parameter)
        {
            return Command != null && Command.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            Command.Execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandReference? commandReference = d as CommandReference;

            if (e.OldValue is RelayCommand oldCommand)
            {
                oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            }
            if (e.NewValue is RelayCommand newCommand)
            {
                newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
            }
        }

        #endregion

        #region Freezable

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}