using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Watcher.Interop
{
    public interface IDataStore
    {
        ObservableCollection<IDataItem> Items { get; }
        ObservableCollection<ISource> Sources { get; }
        string Type { get; }

        List<IDataItem> AddItems(List<IDataItem> items);
        ISource AddSource(ISource source);
        void Initialize(List<IProvider> providers);
        void RemoveSource(ISource source);
        List<IDataItem> Search(string filter);
        void UpdateItem(List<IDataItem> items);
        void UpdateItem(IDataItem item);
        void UpdateSource(ISource source);
    }
}