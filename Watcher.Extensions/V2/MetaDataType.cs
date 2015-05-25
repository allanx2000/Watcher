using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watcher.Extensions.V2
{
    public class MetaDataObject
    {
        public enum Type
        {
            String,
            Selector
        }

        public string ID  { get; private set; }
        public string DisplayName  { get; private set; }
        public Type FieldType { get; private set; }
        public object Value { get; private set; }

        public List<string> SelectorValues { get; private set; }

        public MetaDataObject(string id, string displayName, Type fieldType = Type.String, List<string> selectorValues = null)
        {
            this.ID = id;
            this.DisplayName = displayName;
            this.FieldType = fieldType;
            
            this.SelectorValues = selectorValues;
        }

        public void SetValue(object value)
        {
            this.Value = value;
        }
    }
}
