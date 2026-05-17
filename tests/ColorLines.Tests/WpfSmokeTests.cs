using System.Threading;
using ColorLines.Windows;

namespace ColorLines.Tests;

public sealed class WpfSmokeTests
{
    [Fact]
    public void MainWindowCanBeCreated()
    {
        Exception? thrown = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = new MainWindow
                {
                    ShowInTaskbar = false,
                    WindowState = System.Windows.WindowState.Minimized
                };
                window.Show();
                window.UpdateLayout();
                window.Close();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(thrown);
    }
}
