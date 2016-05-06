using System.Collections.Generic;

namespace Watcher.Interop
{
    public interface ISource2 : ISource
    {
        ServiceProvider Services { get;}
    }
    
}