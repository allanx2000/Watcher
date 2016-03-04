using System;

namespace Watcher.Interop
{
    public interface IDataItem
    {
        string ActionContent { get; }
        DateTime AddedDate { get; }
        int? ID { get; }
        string Name { get; }
        bool New { get; }
        string Provider { get; set; }
        int SourceId { get; }
        string SourceName { get; set; }

        bool Equals(object obj);
        int GetHashCode();
        ISource GetSource();
        IDataItem SetId(int id);
        void SetNew(bool isNew);
    }
}