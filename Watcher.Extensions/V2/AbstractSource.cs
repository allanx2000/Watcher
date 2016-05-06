using System;
using System.Collections.Generic;
using System.Windows.Media;
using Watcher.Interop;

namespace Watcher.Extensions.V2
{

    public abstract class AbstractSource : ISource2
    {   
        public ServiceProvider Services { get; set; }

        public int? ID {get; private set;}
        public string ProviderID {get; private set;}
        public string SourceName { get; private set; }
        
        private Dictionary<string, IMetaDataObject> MetaData = new Dictionary<string, IMetaDataObject>();
        
        public virtual string GetDisplayName()
        {
            return ProviderID + " > " + SourceName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceName">This is used for serialization, it is a key that identifies the type of source; it can also be a friendly-name</param>
        /// <param name="providerId">The provider's key</param>
        protected AbstractSource(string sourceName, string providerId, bool disabled = false)
        {
            ProviderID = providerId;
            SourceName = sourceName;
        }

        /// <summary>
        /// Creates the source using the the GenericSource
        /// </summary>
        /// <param name="src">The Generic Source to get the parameters from</param>
        protected AbstractSource(GenericSource src)
        {
            if (src.ID.HasValue)
                this.SetID(src.ID.Value);

            this.SetProviderID(src.ProviderID);
            this.SetSourceName(src.SourceName);
            this.SetMetaData(src.GetMetaData());
        }

        //TODO: Add to interface, Rename
        //TODO: Change to Add...
        /// <summary>
        /// Adds the MetaDataObject to MetaData, used for templating
        /// </summary>
        /// <param name="meta"></param>
        public void SetMetaData(IMetaDataObject meta) 
        {
                MetaData.Add(meta.ID, meta);
        }

        public bool HasMetadata(string key)
        {
            return MetaData.ContainsKey(key);
        }
        
        /// <summary>
        /// Add a custom metadata value to the source definition
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISource SetMetaDataValue(string key, string value) //Change to object
        {
            MetaData[key].SetValue(value);
            
            return this;
        }

        public ISource ClearMetaDataValue(string key)
        {
            if (MetaData.ContainsKey(key))
                MetaData[key].SetValue(null);

            return this;
        }

        /// <summary>
        /// Gets the value from the metadata
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value or null if not found or not set</returns>
        public object GetMetaDataValue(string key)
        {
            if (MetaData.ContainsKey(key))
                return MetaData[key].Value;
            else return null;
        }
        
        /// <summary>
        /// Gets the entire metadata dictionary including protected
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IMetaDataObject> GetMetaData()
        {
            var clone = new Dictionary<string, IMetaDataObject>(MetaData);

            /*
            foreach (string field in ProtectedValues)
            {
                clone.Remove(field);
            }*/

            return clone;
        }
        

        public ISource SetID(int id)
        {
            ID = id;

            return this;
        }

        public ISource SetProviderID(string providerId)
        {
            ProviderID = providerId;

            return this;
        }

        public ISource SetSourceName(string sourceName)
        {
            SourceName = sourceName;

            return this;
        }

        public override bool Equals(object that)
        {
            if (that is AbstractSource)
                return IsEqual((AbstractSource)that);
            else 
                return false;
        }

        public override int GetHashCode()
        {
            return this.ProviderID.GetHashCode()*13 
                + SourceName.GetHashCode()*7;
        }

        private bool IsEqual(ISource that)
        {
            return this.ProviderID == that.ProviderID
                && this.SourceName == that.SourceName;
        }

        public virtual ISource SetMetaData(Dictionary<string, string> metadata)
        {
            //TODO: Whats this really for?
            //unparsed = metadata;
            return this;
        }


        /// <summary>
        /// Sets the metadata fields. This should contain all fields from the Provider with any values in the Source itself placed into the Value property 
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public virtual ISource SetMetaData(Dictionary<string, IMetaDataObject> metadata)
        {
            if (this.MetaData == null)
                this.MetaData = metadata;
            else
            {
                foreach (var m in metadata)
                {
                    this.MetaData[m.Key] = m.Value;
                }
            }
            return this;
        }

        public virtual void CopyTo(ISource outputSource)
        {
            outputSource.SetMetaData(this.MetaData);
            outputSource.SetProviderID(this.ProviderID);
            outputSource.SetSourceName(this.SourceName);
        }

        /// <summary>
        /// Sets the MetaData from a List
        /// </summary>
        /// <param name="metaData"></param>
        public virtual void SetMetaData(List<IMetaDataObject> metaData)
        {
            Dictionary<string, IMetaDataObject> dict = new Dictionary<string, IMetaDataObject>();
            
            foreach(var i in metaData)
            {
                dict.Add(i.ID, i);
            }

            this.MetaData = dict;

        }
    }
}
