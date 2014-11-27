using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Watcher.Core;
using Watcher.Core.Internal;
using Watcher.Core.Items;

namespace Watcher.Client.WPF.ViewModels
{
    class MainWindowViewModel : ViewModel
    {
        private volatile bool isUpdating = false;
        private DateTime? lastUpdated = null;
        private bool showOld = false;

        private Dictionary<AbstractSource, SourceViewModel> SVMLookup = new Dictionary<AbstractSource, SourceViewModel>();

        private CollectionViewSource sortedView;

        private MTObservableCollection<ItemViewModel> items = new MTObservableCollection<ItemViewModel>();
        private MTObservableCollection<SourceViewModel> sources = new MTObservableCollection<SourceViewModel>();

        #region Properties
        public ICollectionView SortedView { get; private set; }

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

        public string LastUpdatedString
        {
            get
            {
                if (isUpdating)
                    return "Updating...";
                else
                {
                    if (lastUpdated == null)
                        return "NA";
                    else return lastUpdated.Value.ToShortDateString() + " " + lastUpdated.Value.ToShortTimeString();
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

        public SourceViewModel GetSVM(AbstractSource source)
        {
            return SVMLookup[source];
        }


        public MainWindowViewModel(IEnumerable<AbstractSource> sources, IEnumerable<AbstractItem> items)
        {
            PopulateSVMLookup(sources);

            foreach (var i in items)
            {
                this.items.Add(new ItemViewModel(i, SVMLookup[i.GetSource()]));
            }

            CreateSortedItemsView();
        }

        private void CreateSortedItemsView()
        {
            sortedView = new CollectionViewSource();

            sortedView.Filter += DoFilter;

            sortedView.Source = Items;

            sortedView.SortDescriptions.Add(new SortDescription("AddedDate", ListSortDirection.Descending));
            sortedView.SortDescriptions.Add(new SortDescription("SourceName", ListSortDirection.Descending));
            sortedView.SortDescriptions.Add(new SortDescription("NewLabel", ListSortDirection.Descending));
            
            SortedView = sortedView.View;
        }

        void DoFilter(object sender, FilterEventArgs e)
        {
            if (!ShowAll)
            {
                var item = e.Item as ItemViewModel;

                if (item != null && !item.Data.New)
                {
                    e.Accepted = false;
                    return;
                }
            }

            e.Accepted = true;
        }


        private void PopulateSVMLookup(IEnumerable<AbstractSource> sources)
        {
            foreach (var s in sources)
            {
                var svm = new SourceViewModel(s);
                this.sources.Add(svm);

                SVMLookup.Add(s, svm);
            }
        }



        public void AddSource(SourceViewModel svm)
        {
            Sources.Add(svm);
            SVMLookup.Add(svm.Data, svm);
        }

        public void RemoveSource(SourceViewModel svm)
        {
            Sources.Remove(svm);
            SVMLookup.Remove(svm.Data);

            List<ItemViewModel> itemsToRemove = new List<ItemViewModel>();
            
            foreach ( var i in items)
            {
                if (i.SourceName == svm.SourceName)
                    itemsToRemove.Add(i);
            }

            foreach (var i in itemsToRemove)
            {
                items.Remove(i);
            }
        
        }
    }

}
