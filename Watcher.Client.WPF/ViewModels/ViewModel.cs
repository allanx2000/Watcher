using System.ComponentModel;

namespace Watcher.Client.WPF.ViewModels
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void RefreshViewModel()
        {
            foreach (var p in this.GetType().GetProperties())
            {
                RaisePropertyChanged(p.Name);
            }
        }
    }

    public abstract class ViewModel<T> : ViewModel
    {
        private T data;
                
        public T Data
        {
            get
            {
                return data;
            }
        }

        protected ViewModel(T source)
        {
            SetSource(source);
        }

        public virtual void SetSource(T source)
        {
            this.data = source;
            RaisePropertyChanged("Data");

            RefreshViewModel();
        }

      

       

        
    }
}
