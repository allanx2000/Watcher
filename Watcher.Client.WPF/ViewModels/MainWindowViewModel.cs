using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Watcher.Core;
using Watcher.Core.Items;
using System.Linq;
using System.Collections.Specialized;
using Watcher.Extensions.V2;
using Watcher.Extensions.Internal;
using Innouvous.Utils.DialogWindow.Windows;
using Watcher.Interop;

namespace Watcher.Client.WPF.ViewModels
{
    class MainWindowViewModel : ViewModel
    {
        private bool topmost = false;

        public bool TopMost
        {
            get
            {
                return topmost;
            }
            set
            {
                topmost = value;
                RaisePropertyChanged();
            }
        }

        private volatile bool isUpdating = false;
        private DateTime? lastUpdated = null;
        private bool showOld = false;

        private Dictionary<ISource, SourceViewModel> SVMLookup = new Dictionary<ISource, SourceViewModel>();

        private CollectionViewSource sortedView;

        private MTObservableCollection<ItemViewModel> items = new MTObservableCollection<ItemViewModel>();
        private MTObservableCollection<SourceViewModel> sources = new MTObservableCollection<SourceViewModel>();

        #region Properties

        private ItemViewModel selectedItem;
        public ItemViewModel SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;

                DoItemAction.RaiseCanExecuteChanged();

                RaisePropertyChanged("SelectedItem");
            }
        }

        private SourceViewModel selectedSource;
        public SourceViewModel SelectedSource
        {
            get
            {
                return selectedSource;
            }
            set
            {
                selectedSource = value;

                RaisePropertyChanged("SelectedSource");

                editSourceCommand.RaiseCanExecuteChanged();
                removeSourceCommand.RaiseCanExecuteChanged();
            }
        }

        public ICollectionView SortedView
        {
            get
            {
                return sortedView.View;
            }
        }

        public bool UpdateButtonsEnabled
        {
            get
            {
                return !isUpdating;
            }
        }

        public bool IsUpdating
        {
            get
            {
                return isUpdating;
            }
            set
            {
                isUpdating = value;

                RaisePropertyChanged("IsUpdating");
                RaisePropertyChanged("UpdateButtonsEnabled");
                RaisePropertyChanged("LastUpdatedString");

            }
        }

        public DateTime? LastUpdated
        {
            get
            {
                return lastUpdated;
            }
            set
            {
                lastUpdated = value;

                RaisePropertyChanged("LastUpdated");
                RaisePropertyChanged("LastUpdatedString");
            }
        }

        private string searchFilterLower;
        private string searchFilter;

        public string SearchFilter
        {
            get
            {
                return searchFilter;
            }
            set
            {
                searchFilter = value;
                searchFilterLower = value == null ? null : value.ToLower();

                RaisePropertyChanged("SearchFilter");
            }
        }

        public string LatestUpdateMessage { get; private set; }
        public string LastUpdatedString
        {
            get
            {
                if (isUpdating)
                    return LatestUpdateMessage;
                else
                {
                    string message = "Last Updated: ";

                    if (lastUpdated == null)
                    {
                        message += "NA";
                    }
                    else
                    {
                        message += lastUpdated.Value.ToShortDateString() + " " + lastUpdated.Value.ToShortTimeString();
                    }

                    return message;
                }
            }
        }

        public bool ShowAll
        {
            get
            {
                return showOld;
            }
            set
            {
                showOld = value;
                SortedView.Refresh();
                RaisePropertyChanged("SortedView");
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new CommandHelper(DoSearch);
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new CommandHelper(ClearSearch);
            }
        }

        private bool isSearch = false;
        public bool IsSearching
        {
            get
            {
                return isSearch;
            }
            set
            {
                isSearch = value;
                RaisePropertyChanged("IsSearching");
                RaisePropertyChanged("NotSearching");
                RaisePropertyChanged("SearchText");
            }
        }

        public bool NotSearching
        {
            get
            {
                return !IsSearching;
            }
        }

        public string SearchText
        {
            get
            {
                return IsSearching ? "Clear" : "Search";
            }
        }

        public void ClearSearch()
        {
            SearchFilter = null;

            items.Clear();

            var defaultItems = DataManager.Instance().DataStore.Items;

            foreach (var i in defaultItems)
            {
                if (i != null)
                {
                    items.Add(new ItemViewModel(i, SVMLookup[i.GetSource()]));
                }
            }
            
        }

        private void DoSearch()
        {
            if (IsSearching)
                ClearSearch();
            else
            {
                if (String.IsNullOrEmpty(SearchFilter.Trim()))
                    throw new Exception("No search string entered");

                items.Clear();
                List<IDataItem> results = DataManager.Instance().DataStore.Search(SearchFilter);

                foreach (var i in results)
                {
                    Items.Add(new ItemViewModel(i, SVMLookup[i.GetSource()]));
                }
            }
            
            SortedView.Refresh();
            IsSearching = !IsSearching;

            RefreshViewModel();
        }


        public MTObservableCollection<ItemViewModel> Items
        {
            get
            {
                return items;
            }
        }

        public MTObservableCollection<SourceViewModel> Sources
        {
            get
            {
                return sources;
            }
        }

        #endregion

        #region Commands

        #region Sources

        private CommandHelper removeSourceCommand;

        public CommandHelper RemoveSourceCommand
        {
            get
            {
                if (removeSourceCommand == null)
                {
                    removeSourceCommand = new CommandHelper(RemoveSource, CanEditSource);
                }
                return removeSourceCommand;
            }
        }

        private void RemoveSource(object sender)
        {
            if (SelectedSource != null)
            {

                DataManager.Instance().DataStore.RemoveSource(SelectedSource.Data);
                RemoveSource(SelectedSource);
            }
        }

        private void RemoveSource(SourceViewModel svm)
        {
            Sources.Remove(svm);
            SVMLookup.Remove(svm.Data);

            List<ItemViewModel> itemsToRemove = new List<ItemViewModel>();

            foreach (var i in items)
            {
                if (i.SourceName == svm.SourceName)
                    itemsToRemove.Add(i);
            }

            foreach (var i in itemsToRemove)
            {
                items.Remove(i);
            }

        }

        private CommandHelper editSourceCommand;

        public CommandHelper EditSourceCommand
        {
            get
            {
                if (editSourceCommand == null)
                    editSourceCommand = new CommandHelper(EditSource, (sender) => CanEditSource(sender));

                return editSourceCommand;
            }
        }

        private bool CanEditSource(object sender)
        {
            return SelectedSource != null;
        }

        private void EditSource(object sender)
        {
            if (SelectedSource != null)
            {
                SourceEditor se = new SourceEditor(SelectedSource);

                se.ShowDialog();
            }
        }

        private CommandHelper addNewSourceCommand;

        public CommandHelper AddNewSourceCommand
        {
            get
            {
                if (addNewSourceCommand == null)
                    addNewSourceCommand = new CommandHelper(new Action<object>(AddSource));

                return addNewSourceCommand;
            }
        }

        public void AddSource(object sender)
        {
            SourceEditor se = new SourceEditor();

            try
            {
                se.ShowDialog();

                var svm = se.GetSourceViewModel();

                if (svm != null)
                {
                    Sources.Add(svm);
                    SVMLookup.Add(svm.Data, svm);
                }
            }
            catch (Exception ex)
            {
                MessageBoxFactory.ShowError(ex);
            }
        }

        #endregion

        #region Items

        public ICommand MarkSelectedCommand
        {
            get
            {
                return new CommandHelper(MarkSelected);
            }
        }

        private void MarkSelected(object selected)
        {
            List<IDataItem> updated = new List<IDataItem>();

            var items = selected as System.Collections.IList;

            foreach (ItemViewModel ivm in items)
            {
                if (ivm.Data.New)
                {
                    ivm.Data.SetNew(false);
                    updated.Add(ivm.Data);
                }
            }

            DataManager.Instance().DataStore.UpdateItem(updated);
            SortedView.Refresh();
        }

        public ICommand MarkAllCommand
        {
            get
            {
                return new CommandHelper(MarkAll);
            }
        }

        private void MarkAll()
        {
            if (MessageBox.Show("Mark all items as read?", "Mark All Read", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            List<IDataItem> updated = new List<IDataItem>();
            foreach (ItemViewModel ivm in items)
            {
                if (ivm.Data.New)
                {
                    ivm.Data.SetNew(false);
                    updated.Add(ivm.Data);
                }
            }

            DataManager.Instance().DataStore.UpdateItem(updated);
            SortedView.Refresh();
        }

        private CommandHelper doItemAction;
        public CommandHelper DoItemAction
        {
            get
            {
                if (doItemAction == null)
                {
                    doItemAction = new CommandHelper(PerformItemAction, (sender) => SelectedItem != null);
                }

                return doItemAction;
            }
        }

        public void PerformItemAction(object sender)
        {
            var o = SelectedItem;

            if (o != null)
            {
                var provider = DataManager.Instance().GetProviders().First(x => x.ProviderId == o.Provider);
                if (provider != null)
                {
                    provider.DoAction(o.Data);
                    o.Data.SetNew(false);
                    DataManager.Instance().DataStore.UpdateItem(o.Data);

                    SortedView.Refresh();
                }
            }
        }

        #endregion

        #region Other

        public ICommand ShowLastUpdatedCommand
        {
            get
            {
                return new CommandHelper(ShowLastUpdated);
            }
        }

        private void ShowLastUpdated()
        {
            string message = String.Join("\r\n", DataManager.Instance().Messages);

            var opts = DialogControlOptions.SetTextBoxMessageOptions(message, false);

            var window = new Innouvous.Utils.DialogWindow.Windows.SimpleDialogWindow(opts);
            window.Title = "Status";

            window.ShowDialog();
        }
        
        #endregion
        #endregion

        #region Items Update Logic

        public CommandHelper UpdateCommand
        {
            get
            {
                return new CommandHelper((obj) => DoUpdate());
            }
        }

        private MTObservableCollection<string> updateMessages = new MTObservableCollection<string>();

        private Properties.Settings AppConfigs = Properties.Settings.Default;

        public void DoUpdate()
        {
            updateMessages.Clear();

            IsUpdating = true;

            DataManager.Instance().UpdateItems(AppConfigs.UpdateTimeout, OnFinishedUpdating);
        }

        public void AbortUpdate()
        {
            if (IsUpdating)
            {
                DataManager.Instance().AbortUpdate();
                
                IsUpdating = false;
            }
        }


        //This just adds the new items to the View
        private void OnFinishedUpdating(bool success, List<IDataItem> newItems, object data)
        {
            LastUpdated = DateTime.Now;
            IsUpdating = false;

            if (success)
            {
                foreach (var i in newItems)
                {
                    try
                    {
                        var svm = GetSVM(i.GetSource());
                        Items.Add(new ItemViewModel(i, svm));
                    }
                    catch (Exception ex)
                    {
                        //Only output errors
                        DataManager.Instance().AddMessage(ex.Message);
                    }
                }
            }
        }

        private void UpdateItemButton_Click(object sender, RoutedEventArgs e)
        {
            DoUpdate();
        }

        #endregion


        public SourceViewModel GetSVM(ISource source)
        {
            return SVMLookup[source];
        }


        public MainWindowViewModel()
        {
            var sources = DataManager.Instance().DataStore.Sources;

            PopulateSVMLookup(sources);

            DataManager.Instance().Messages.CollectionChanged += Messages_CollectionChanged;

            LoadDefaultItems();
        }
        private void LoadDefaultItems()
        {
            var items = DataManager.Instance().DataStore.Items;

            foreach (var i in items)
            {
                if (i != null)
                {
                    this.items.Add(new ItemViewModel(i, SVMLookup[i.GetSource()]));
                }
            }

            CreateSortedItemsView();
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null && e.NewItems != null && e.NewItems.Count > 0)
            {
                LatestUpdateMessage = (string)e.NewItems[0];

                RaisePropertyChanged("LastUpdatedString");
            }
        }

        private void CreateSortedItemsView()
        {
            sortedView = new CollectionViewSource();

            sortedView.Filter += DoFilter;

            sortedView.Source = Items;

            //First to Last
            sortedView.SortDescriptions.Add(new SortDescription("NewLabel", ListSortDirection.Descending));
            sortedView.SortDescriptions.Add(new SortDescription("AddedDate", ListSortDirection.Descending));
            sortedView.SortDescriptions.Add(new SortDescription("SourceName", ListSortDirection.Ascending));
            sortedView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));

            RaisePropertyChanged("SortedView");
        }

        void DoFilter(object sender, FilterEventArgs e)
        {

            var item = e.Item as ItemViewModel;

            if (item != null)
            {

                if (!String.IsNullOrEmpty(SearchFilter))
                {
                    if (!item.Title.ToLower().Contains(searchFilterLower))
                    {
                        e.Accepted = false;
                        return;
                    }
                }

                if (!ShowAll)
                {
                    if (!item.Data.New)
                    {
                        e.Accepted = false;
                        return;
                    }
                }
            }

            e.Accepted = true;
        }


        private void PopulateSVMLookup(IEnumerable<ISource> sources)
        {
            foreach (var s in sources)
            {
                var svm = new SourceViewModel(s);
                this.sources.Add(svm);

                SVMLookup.Add(s, svm);
            }
        }



    }

}
