using System.Collections.Generic;
using Watcher.Interop;

namespace Watcher.Extensions.V2
{
    public abstract class AbstractProvider : IProvider
    {
        private readonly string providerId;

        public bool HasUrlField { get; protected set; }
        public bool HasUniqueName { get; protected set; }
     
        protected AbstractProvider(string providerId, bool hasUrlField = false, bool hasUniqueName = false)
        {
            this.providerId = providerId;
            this.HasUrlField = hasUrlField;
            this.HasUniqueName = hasUniqueName;
        }

        public string ProviderId
        {
            get
            {
                return GetProviderId();
            }
        }

        public string GetProviderId()
        {
            return providerId;
        }

        /// <summary>
        /// This returns the ALL fields that the Provider requires/uses
        /// </summary>
        /// <returns></returns>
        public virtual List<IMetaDataObject> GetMetaFields()
        {
            return new List<IMetaDataObject>();
        }
        public ISource CreateNewSource(string name, string url, Dictionary<string, IMetaDataObject> metaData)
        {
            var values = new List<IMetaDataObject>(metaData.Values);

            return CreateNewSource(name, url, values);
        }

        //TODO: CHANGE
        public ISource CreateNewSource(string name, string url, List<IMetaDataObject> metaData)
        {
            var s = DoCreateNewSource(name, url, metaData);
            
            //s.SetMetaData(metaData);

            return s;
        }

        /// <summary>
        /// Called from abstract class to create a new source.
        /// The metaData will be entered by the calling function CreateNewSource
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="metaData">Meta to be used for reference only</param>
        /// <returns></returns>
        protected abstract ISource DoCreateNewSource(string name, string url, List<IMetaDataObject> metaData);
        
        public List<IDataItem> CheckForNewItems(ISource source)
        {
            if (this.providerId != source.ProviderID)
                return null; //throw new Exception(String.Format("Source is not supported by this provider."));
            else
                return GetNewItems(source);
        }

        protected abstract List<IDataItem> GetNewItems(ISource source);

        public bool CanCheck(ISource source)
        {
            return source.ProviderID == this.providerId;
        }

        public virtual ISource CastSource(ISource src)
        {
            return src;
        }

        public abstract void DoAction(IDataItem item);

    }
}
