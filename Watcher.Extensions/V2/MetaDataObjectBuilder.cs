using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;

namespace Watcher.Extensions.V2
{
    public class MetaDataObjectBuilder
    {
        private MetaDataObject template;

        public MetaDataObjectBuilder(string id, string displayName, MetaDataObject.Type type = MetaDataObject.Type.String, List<string> selectValues = null)
        {
            template = new MetaDataObject(id, displayName, type, selectValues);
        }

        public MetaDataObject Create()
        {
            return
                new MetaDataObject(template.ID, template.DisplayName, template.FieldType, template.SelectorValues);
        }
    }
}
