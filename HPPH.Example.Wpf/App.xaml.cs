using System.Windows;
using System.Windows.Threading;

namespace HPPHExample;

public partial class App
{
    #region Methods

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    #endregion
}
