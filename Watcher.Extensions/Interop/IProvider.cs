using System.Collections.Generic;

namespace Watcher.Interop
{
    public interface IProvider
    {
        bool HasUniqueName { get; }
        bool HasUrlField { get; }
        string ProviderId { get; }

        bool CanCheck(ISource source);
        ISource CastSource(ISource src);
        List<IDataItem> CheckForNewItems(ISource source);
        ISource CreateNewSource(string name, string url, List<IMetaDataObject> metaData);
        ISource CreateNewSource(string name, string url, Dictionary<string, IMetaDataObject> metaData);
        void DoAction(IDataItem item);
        List<IMetaDataObject> GetMetaFields();
        string GetProviderId();
    }
}