using System.Collections.Generic;

namespace Watcher.Interop
{
    public interface ISource
    {
        bool Disabled { get; set; }
        int? ID { get; }
        string ProviderID { get; }
        string SourceName { get; }

        //bool MetadataUnparsed { get; }

        ISource ClearMetaDataValue(string key);
        void CopyTo(ISource outputSource);
        bool Equals(object that);
        string GetDisplayName();
        int GetHashCode();
        Dictionary<string, IMetaDataObject> GetMetaData();
        object GetMetaDataValue(string key);
        ISource SetID(int id);
        void SetMetaData(List<IMetaDataObject> metaData);
        ISource SetMetaData(Dictionary<string, IMetaDataObject> metadata);
        ISource SetMetaDataValue(string key, string value);
        ISource SetProviderID(string providerId);
        ISource SetSourceName(string sourceName);
    }
}