using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Watcher.Core.Internal
{
    public class MTObservableCollection<T> : ObservableCollection<T>
    {
        // Override the event so this class can access it
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public MTObservableCollection(IEnumerable<T> collection) : base(collection) { }
        public MTObservableCollection(List<T> collection) : base(collection) { }

        public MTObservableCollection() : this(new List<T>())
        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Be nice - use BlockReentrancy like MSDN said
            using (BlockReentrancy())
            {
                var eventHandler = CollectionChanged;
                if (eventHandler != null)
                {
                    Delegate[] delegates = eventHandler.GetInvocationList();

                    // Walk thru invocation list
                    foreach (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        var dispatcherObject = handler.Target as DispatcherObject;

                        // If the subscriber is a DispatcherObject and different thread
                        if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)

                            // Invoke handler in the target dispatcher's thread
                            dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                          handler, this, e);
                        else // Execute handler as is
                            handler(this, e);
                    }
                }
            }
        }
    }
}