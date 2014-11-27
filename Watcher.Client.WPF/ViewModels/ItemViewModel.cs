using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Watcher.Core;
using Watcher.Core.Items;

namespace Watcher.Client.WPF.ViewModels
{
    public class ItemViewModel : ViewModel<AbstractItem>
    {
        private readonly SourceViewModel svm;
        
        public ItemViewModel(AbstractItem item, SourceViewModel sourceViewModel)
            : base(item)
        {
            svm = sourceViewModel;
            svm.PropertyChanged += ((e, args) => RefreshViewModel());
        }


        public SolidColorBrush UpdatesBrush
        {
            get
            {
                return svm.Data.UpdatesColorBrush;
            }
        }

        public string NewLabel
        {
            get
            {
                return Data.New ? "NEW" : "";
            }
        }

        public string Provider
        {
            get
            {
                return Data.Provider;
            }
        }

        public string SourceName
        {
            get
            {
                return Data.SourceName;
            }
        }
        public string Title
        {
            get
            {
                return Data.Name;
            }
        }


        public string AddedDateString
        {
            get
            {
                return Data.AddedDate == null ?
                    "" :
                    Data.AddedDate.ToString("M/d/yyyy h:mm tt");
            }
        }

        public DateTime AddedDate
        {
            get
            {
                return Data.AddedDate;
            }
        }

        /*public override void RefreshViewModel()
        {
            base.RefreshViewModel();

            RaisePropertyChanged("AddedDateString");
            RaisePropertyChanged("NewLabel");
            RaisePropertyChanged("UpdatesBrush");
        }*/

    }
}
