using Watcher.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Provider.Dummy
{    public class DummySource : AbstractSource
    {

        private const string PROVIDER = "DummyProvider";

        public DummySource(string categoryName)
            : base(categoryName, PROVIDER)
        {}
    }
}
