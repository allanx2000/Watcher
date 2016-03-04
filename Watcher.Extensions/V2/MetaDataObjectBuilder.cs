using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Extensions.V2
{
    public class MetaDataObjectBuilder
    {
        private MetaDataObject template;

        public MetaDataObjectBuilder(string id, string displayName, MetaDataObjectType type = MetaDataObjectType.String, List<string> selectValues = null)
        {
            template = new MetaDataObject(id, displayName, type, selectorValues: selectValues);
        }

        public MetaDataObject Create()
        {
            return
                new MetaDataObject(template.ID, template.DisplayName, template.FieldType, selectorValues: template.SelectorValues);
        }

        public MetaDataObject Create(object value)
        {
            var mdo = Create();
            mdo.SetValue(value);

            return mdo;
        }
    }
}
