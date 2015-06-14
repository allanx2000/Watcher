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
            Selector,
            NA
        }

        //TODO: Field Required or Optional
        
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

        public void SetDisplayName(string name)
        {
            DisplayName = name;
        }

        public void SetFieldType(Type type)
        {
            FieldType = type;
        }

        public void SetSelectorValues(List<string> list)
        {
            SelectorValues = list;
        }

        
        public string GetValueAsString()
        {
            return Value == null? null : Value.ToString();
        }


        public static MetaDataObject FindIn(List<MetaDataObject> metaData, string id)
        {
            var mdo = metaData.FirstOrDefault(x => x.ID == id);
            if (mdo == null)
                throw new Exception(id + "is not found in metadata");

            return mdo;
        }

        public static MetaDataObject FindIn(Dictionary<string, MetaDataObject> metaData, string id)
        {
            if (!metaData.ContainsKey(id))
                throw new Exception(id + "is not found in metadata");

            return metaData[id];
        }

        public static Dictionary<string, string> ToDictionary(Dictionary<string, MetaDataObject> dictionary)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var i in dictionary.Values)
            {
                dict.Add(i.ID, i.GetValueAsString());
            }

            return dict;
        }
    }
}
