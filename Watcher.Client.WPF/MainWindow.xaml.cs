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
        
        private DispatcherTimer timer;
       
        private Properties.Settings AppConfigs = Properties.Settings.Default;
            
        private MainWindowViewModel viewModel;
        
        private delegate void AddedItemsHandler(List<AbstractItem> newItems, string error);

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


        public MainWindow()
        {

            InitializeComponent();

            //Loads the window with default configurations
            RunConfigsWindow cf = new RunConfigsWindow();

            while (true)
            {
                try
                {

                    if (cf.IsFirstRun || cf.Retry)
                    {
                        cf.ShowDialog();
                    }

                    LoadFromConfigurations();
                    cf.Retry = false;

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
                        cf.Retry = true; 
                }
            }

        }

        /// <summary>
        /// Load the application using the configuration settings in the program properties
        /// </summary>
        private void LoadFromConfigurations()
        {
            //var datastore = new SQLiteDataStore(AppConfigs.DataStoreFile);

            var datastore = new SQLiteDataStoreV2(AppConfigs.DataStoreFile);
            var providerLoader = new SuperProvidersLoader(AppConfigs.ProvidersPath);
            
            var providers = providerLoader.GetProviders();

            DataManager.Initialize(datastore, providers);

            //Create the View Model
            viewModel = new MainWindowViewModel();
            
            this.DataContext = viewModel;

            viewModel.RefreshViewModel();

            //Begin update timer
            ResetUpdateTimer();
        }

        private void ItemsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.PerformItemAction(sender);
        }

        private AbstractProvider GetProvider(AbstractItem o)
        {
            return DataManager.Instance().GetProviders().First(p => p.GetProviderId() == o.Provider);
        }

        private void MarkSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            List<AbstractItem> updated = new List<AbstractItem>();
            foreach (ItemViewModel ivm in ItemsListBox.SelectedItems)
            {
                if (ivm.Data.New)
                { 
                    ivm.SetSource(ivm.Data.SetNew(false));
                    updated.Add(ivm.Data);
                }
            }

            DataManager.Instance().DataStore.UpdateItem(updated);
            viewModel.SortedView.Refresh();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (viewModel != null && viewModel.IsUpdating)
            {
                e.Cancel = true;
            }
            else Application.Current.Shutdown(0);
        }

        private void MarkAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Mark all items as read?", "Mark All Read", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            
            List<AbstractItem> updated = new List<AbstractItem>();
            foreach (ItemViewModel ivm in ItemsListBox.Items)
            {
                if (ivm.Data.New)
                {
                    ivm.SetSource(ivm.Data.SetNew(false));
                    updated.Add(ivm.Data);
                }
            }

            DataManager.Instance().DataStore.UpdateItem(updated);
            viewModel.SortedView.Refresh();
        }

        private void ToggleSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            SourcesGroupBox.Visibility = SourcesGroupBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new RunConfigsWindow();
            window.ShowDialog();

            if (!window.Cancelled)
            {
                LoadFromConfigurations();
            }
        }

        private void LastUpdatedLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string message = String.Join("\r\n", DataManager.Instance().Messages);

            var opts = DialogControlOptions.SetTextBoxMessageOptions(message, false);

            var window = new Innouvous.Utils.DialogWindow.Windows.SimpleDialogWindow(opts);
            window.Title = "Status";

            window.ShowDialog();
        }
    }
}
