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
        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (Checker.AlreadyRunning)
                Environment.Exit(0);
        }
    }
}
