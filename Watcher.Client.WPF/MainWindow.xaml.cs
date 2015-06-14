using Watcher.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Watcher.Core.Items;
using Watcher.Client.WPF.ViewModels;
using Watcher.Core.Loaders;
using Watcher.DataStore.SQLite;
using System.Windows.Threading;
using Innouvous.Utils;
using Innouvous.Utils.DialogWindow;
using Innouvous.Utils.DialogWindow.Windows;
using Watcher.Extensions.V2;

namespace Watcher.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Auto Updater
        private DispatcherTimer timer;

        public void ResetUpdateTimer()
        {
            if (timer != null)
            {
                timer.Stop();

                timer.Interval = new TimeSpan(0, AppConfigs.UpdateFrequency, 0);
            }
            else
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, AppConfigs.UpdateFrequency, 0);
                timer.Tick += (sender, e) =>
                {
                    if (!viewModel.IsUpdating)
                        viewModel.DoUpdate();
                };

            }

            timer.Start();
        }

        #endregion

        private Properties.Settings AppConfigs = Properties.Settings.Default;
        private MainWindowViewModel viewModel;

        private delegate void AddedItemsHandler(List<AbstractItem> newItems, string error);

        public MainWindow()
        {

            InitializeComponent();

            //Loads the window with default configurations
            RunConfigsWindow cf = new RunConfigsWindow();

            while (true) //Keep trying on errors
            {
                try
                {
                    if (cf.IsFirstRun || cf.ShouldRetry)
                    {
                        cf.ShowDialog();
                    }

                    LoadFromConfigurations();
                    cf.ShouldRetry = false;

                    viewModel.DoUpdate();

                    break;
                }
                catch (Exception e)
                {
                    var result = MessageBox.Show(
                        "Error occured while initializing program: " + e.Message + ". Do you want to modify the configurations?",
                        "Error Loading Configurations",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.No)
                        Application.Current.Shutdown(0);
                    else
                        cf.ShouldRetry = true;
                }
            }

        }

        /// <summary>
        /// Load the application using the configuration settings in the program properties
        /// </summary>
        private void LoadFromConfigurations()
        {
            AbstractDataStore datastore = new SQLiteDataStoreV2(AppConfigs.DataStoreFile);
            IProviderLoader providerLoader = new SuperProvidersLoader(AppConfigs.ProvidersPath);

            var providers = providerLoader.GetProviders();

            DataManager.Initialize(datastore, providers);

            //Create the View Model
            viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
            viewModel.RefreshViewModel();

            //Begin update timer
            ResetUpdateTimer();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TODO: Kill update?
            if (viewModel != null && viewModel.IsUpdating)
            {
                viewModel.AbortUpdate();
            }
            
            Application.Current.Shutdown(0);
        }

        /*
        //Not used?
        private void ToggleSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            SourcesGroupBox.Visibility = SourcesGroupBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }*/


        //Easier here
        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new RunConfigsWindow();
            window.ShowDialog();

            if (!window.Cancelled)
            {
                LoadFromConfigurations();
            }
        }


        //This cannot be done easily with Command binding
        private void ItemsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.PerformItemAction(sender);
        }
    }
}
