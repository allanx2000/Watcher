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
                        DoUpdate();
                };

            }

            timer.Start();
        }

        public MainWindow()
        {

            InitializeComponent();

            try
            {
                //Loads the window with default configurations
                RunConfigsWindow cf = new RunConfigsWindow();
                
                if (cf.IsFirstRun)
                {
                    cf.ShowDialog();

                    if (cf.Cancelled)
                    {
                        throw new Exception("Settings not configured");
                    }
                }

                LoadFromConfigurations();

            }
            catch (Exception e)
           { 
                MessageBox.Show(e.Message);

                Application.Current.Shutdown(0);
            }

            DoUpdate();

        }

        /// <summary>
        /// Load the application using the configuration settings in the program properties
        /// </summary>
        private void LoadFromConfigurations()
        {
            var datastore = new SQLiteDataStore(AppConfigs.DataStoreFile);

            var providerLoader = new ProvidersLoader(AppConfigs.ProvidersPath);
            var providers = providerLoader.GetProviders();

            DataManager.Initialize(datastore, providers);

            //Create the View Model
            var sources = DataManager.Instance().DataStore.Sources;

            var items = DataManager.Instance().DataStore.Items;

            viewModel = new MainWindowViewModel(sources, items);
            
            this.DataContext = viewModel;

            viewModel.RefreshViewModel();

            //Begin update timer
            ResetUpdateTimer();
        }


        private void AddSourceButton_Click(object sender, RoutedEventArgs e)
        {
            SourceEditor se = new SourceEditor();

            try
            {
                se.ShowDialog();

                var svm = se.GetSourceViewModel();

                if (svm != null)
                    viewModel.AddSource(svm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoveSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SourcesListBox.SelectedItem != null)
            {
                SourceViewModel svm = (SourceViewModel) SourcesListBox.SelectedItem;

                DataManager.Instance().DataStore.RemoveSource(svm.Data);
                viewModel.RemoveSource(svm);
            }

        }

        private void EditSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SourcesListBox.SelectedItem != null)
            {
                SourceViewModel svm = (SourceViewModel)SourcesListBox.SelectedItem;

                SourceEditor se = new SourceEditor(svm);

                se.ShowDialog();


            }
        }

        private void DoUpdate()
        {
            viewModel.IsUpdating = true;

            DataManager.Instance().UpdateBooks(AppConfigs.UpdateTimeout,
                (addedItems, error) =>
                {
                    Dispatcher.Invoke(new AddedItemsHandler(OnUpdateAction), addedItems, error);
                });
        }

        private void OnUpdateAction(List<AbstractItem> newItems, string error)
        {
            viewModel.LastUpdated = DateTime.Now;
                    viewModel.IsUpdating = false;

                    foreach (var i in newItems)
                    {
                        try
                        {

                            var svm = viewModel.GetSVM(i.GetSource());
                            viewModel.Items.Add(new ItemViewModel(i, svm));
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex.Message);
                        }
                    }

                    //show errors
                    if (error != null)
                        MessageBox.Show(error);
        }

        private void UpdateItemButton_Click(object sender, RoutedEventArgs e)
        {
            DoUpdate();

        }

        private void ItemsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var o = ItemsListBox.SelectedItem as ItemViewModel;

            if (o != null)
            {
                GetProvider(o.Data).DoAction(o.Data);

                //Process.Start(o.URL);

                o.Data.SetNew(false);
                DataManager.Instance().DataStore.UpdateItem(o.Data);
                viewModel.SortedView.Refresh();
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ShowAllCheckBox.GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (viewModel.IsUpdating)
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

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new TestWindow();

            win.ShowDialog();
        }
    }
}
