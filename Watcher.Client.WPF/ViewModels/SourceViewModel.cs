using Innouvous.Utils.MVVM;
using System.Windows.Media;
using Watcher.Core;
using Watcher.Extensions;

namespace Watcher.Client.WPF.ViewModels
{
    public class SourceViewModel : ViewModel<AbstractSource>
    {
      
        public string DisplayName
        {
            get
            {
                return Data.GetDisplayName();
            }
        }

        public SolidColorBrush UpdatesColorBrush
        {
            get
            {
                return Data.UpdatesColorBrush;
            }
        }

        public string SourceName
        {
            get
            {
                return Data.SourceName;
            }
        }

        public SourceViewModel(AbstractSource source) : base(source)
        {

        }

        public override void RefreshViewModel()
        {
            base.RefreshViewModel();

        }

    }
}
