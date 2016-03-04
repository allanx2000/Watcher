using System.Collections.Generic;

namespace Watcher.Interop
{
    public interface IMetaDataObject
    {
        string DisplayName { get; }
        MetaDataObjectType FieldType { get; }
        string ID { get; }
        List<string> SelectorValues { get; }
        object Value { get; }

        string GetValueAsString();
        void SetDisplayName(string name);
        void SetFieldType(MetaDataObjectType type);
        void SetSelectorValues(List<string> list);
        void SetValue(object value);
    }
}