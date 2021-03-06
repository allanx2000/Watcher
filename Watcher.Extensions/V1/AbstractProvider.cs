﻿using System.Collections.Generic;

namespace Watcher.Extensions.V1
{
    [Obsolete]
    public abstract class AbstractProvider
    {
        private readonly string providerId;

        protected AbstractProvider(string providerId)
        {
            this.providerId = providerId;
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

        public virtual List<string> GetMetaFields()
        {
            return new List<string>();
        }

        public virtual SourceOptions GetSourceOptions()
        {
            return SourceOptions.CreateFromParameters();
        }

        public AbstractSource CreateNewSource(string name, string url, Dictionary<string,string> metaData)
        {
            var s = DoCreateNewSource(name, url, metaData);
            
            s.SetMetaData(metaData);

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
        protected abstract AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData);
        
        public List<AbstractItem> CheckForNewItems(AbstractSource source)
        {
            if (this.providerId != source.ProviderID)
                return null; //throw new Exception(String.Format("Source is not supported by this provider."));
            else
                return GetNewItems(source);
        }

        protected abstract List<AbstractItem> GetNewItems(AbstractSource source);

        public bool CanCheck(AbstractSource source)
        {
            return source.ProviderID == this.providerId;
        }

        public virtual AbstractSource CastSource(GenericSource src)
        {
            return src;
        }

        public abstract void DoAction(AbstractItem item);

        
        #region Upgrade Version Support
        
        internal List<AbstractItem> SupportGetNewItems(AbstractSource source)
        {
            return GetNewItems(source);
        }


        #endregion
    }
}
