using Watcher.Core;
using System;
using System.Collections.Generic;
using Watcher.Core.Items;
using System.Windows;

namespace Watcher.Provider.Dummy
{
    public class DummyProvider : AbstractProvider
    {
        private const string PROVIDER = "DummyProvider";

        public DummyProvider() : base(PROVIDER)
        {

        }

        //TODO: Get Source Required Meta Fields

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            var s = new DummySource(name);
            
            return s;
        }

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            return new List<AbstractItem>();
        }

        public override List<string> GetMetaFields()
        {
            throw new NotImplementedException();
        }

        public override void DoAction(AbstractItem item)
        {
            //TODO: Create Plugin library
            //MessageAction().Do(o);
            MessageBox.Show(item.SourceName);
        }

        public override SourceOptions GetSourceOptions()
        {
            return new SourceOptions();
        }
    }

    
}
