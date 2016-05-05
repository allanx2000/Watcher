using Innouvous.Utils.SingleInstance;
using System;
using System.Windows;

namespace Watcher.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Checker.AlreadyRunning)
                Environment.Exit(0);
        }
    }
}
