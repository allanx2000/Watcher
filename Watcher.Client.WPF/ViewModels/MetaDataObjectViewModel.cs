using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Watcher.Extensions.V2;

namespace Watcher.Client.WPF.ViewModels
{
    public class MetaDataObjectViewModel : ViewModel
    {
        private MetaDataObject data;

        public MetaDataObject Data
        {
            get
            {
                return data;
            }
        }

        public MetaDataObjectViewModel(MetaDataObject data)
        {
            this.data = data;
        }

        public Visibility TextBoxVisibility
        {
            get
            {
                return data.FieldType == MetaDataObject.Type.String ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility SelectorVisibility
        {
            get
            {
                return data.FieldType == MetaDataObject.Type.Selector ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string DisplayName
        {
            get
            {
                return data.DisplayName;
            }
        }

        public List<string> SelectorValues
        {
            get
            {
                return data.SelectorValues;
            }
        }

        public object Value
        {
            get
            {
                return data.Value;
            }
            set
            {
                data.SetValue(value);
                RaisePropertyChanged("Value");
            }
        }



    }
}
