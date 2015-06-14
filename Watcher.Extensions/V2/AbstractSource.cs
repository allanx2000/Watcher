using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Watcher.Extensions.V2
{

    public abstract class AbstractSource
    {   
        public int? ID {get; private set;}
        public string ProviderID {get; private set;}
        public string SourceName { get; private set; }
        
        private Dictionary<string, MetaDataObject> MetaData = new Dictionary<string, MetaDataObject>();

        public static readonly Color DefaultColor = Colors.Black;

        #region Meta Values
        public const string UPDATES_COLOR = "UpdatesColor";
        public const string URL = "Url";

        private static readonly List<string> ProtectedValues = new List<string>()
        {
            UPDATES_COLOR,
            URL
        };

        public string Url
        {
            get
            {
                return MetaData.ContainsKey(URL) && MetaData[URL].Value  != null? MetaData[URL].Value.ToString() : null;
            }
        }

        private void AddProtectedMetaData(string key, object value)
        {
            if (!MetaData.ContainsKey(key))
            {
                MetaData.Add(key, new MetaDataObject(key, key.ToString()));
            }

            MetaData[key].SetValue(value);
        }

        private string SerializeColor(Color color)
        {
            //A R G B
            return String.Join(" ", color.A, color.R, color.G, color.B);
        }

        public SolidColorBrush UpdatesColorBrush
        {
            get
            {
                return new SolidColorBrush(UpdatesColor);
            }
        }

        public Color UpdatesColor
        {
            get
            {
                try
                {
                    if (MetaData.ContainsKey(UPDATES_COLOR))
                        return Deserialize(MetaData[UPDATES_COLOR].Value.ToString());
                    else
                        return Colors.Black;
                }
                catch
                {
                    ClearMetaDataValue(UPDATES_COLOR);

                    return Colors.Black;
                }
            }
        }

        public AbstractSource SetUrl(string url)
        {
            var valid = true;

            if (valid)
            {
                this.AddProtectedMetaData(URL, url);
            }
            else throw new Exception("Not a valid URL");
            
            return this;
        }

        public AbstractSource SetUpdatesColor(Color color)
        {
            this.AddProtectedMetaData(UPDATES_COLOR, SerializeColor(color));

            return this;
        }

        
        private Color Deserialize(string p)
        {
            var parts = p.Split(' ');
            int idx = 0;

            Color c = new Color();
            
            c.A = Convert.ToByte(parts[idx++]);
            c.R = Convert.ToByte(parts[idx++]);
            c.G = Convert.ToByte(parts[idx++]);
            c.B = Convert.ToByte(parts[idx++]);

            return c;
        }

        #endregion

        public virtual string GetDisplayName()
        {
            return ProviderID + " > " + SourceName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceName">This is used for serialization, it is a key that identifies the type of source; it can also be a friendly-name</param>
        /// <param name="providerId">The provider's key</param>
        protected AbstractSource(string sourceName, string providerId)
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

        
        /// <summary>
        /// Add a custom metadata value to the source definition
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public AbstractSource SetMetaDataValue(string key, string value)
        {
            //TODO: Add to initializer, should inialize the Dictionary with the MetaDataObjects on create
            /*if (!ProtectedValues.Contains(key))
            else throw new Exception(key + " is protected and cannot be explicitly set");
            */
            
            MetaData[key].SetValue(value);
            
            return this;
        }

        public AbstractSource ClearMetaDataValue(string key)
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
        /// Gets the entire metadata dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,MetaDataObject> GetMetaData()
        {
            var clone = new Dictionary<string, MetaDataObject>(MetaData);
            foreach (string field in ProtectedValues)
            {
                clone.Remove(field);
            }

            return clone;
        }


        public AbstractSource SetID(int id)
        {
            ID = id;

            return this;
        }

        public AbstractSource SetProviderID(string providerId)
        {
            ProviderID = providerId;

            return this;
        }

        protected AbstractSource SetURL(string url)
        {
            AddProtectedMetaData(URL, url);

            return this;
        }

        public AbstractSource SetSourceName(string sourceName)
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

        private bool IsEqual(AbstractSource that)
        {
            return this.ProviderID == that.ProviderID
                && this.SourceName == that.SourceName;
        }

        /// <summary>
        /// Sets the metadata fields. This should contain all fields from the Provider with any values in the Source itself placed into the Value property 
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public AbstractSource SetMetaData(Dictionary<string, MetaDataObject> metadata)
        {
            this.MetaData = metadata;

            return this;
        }

        public void CopyTo(AbstractSource outputSource)
        {
            outputSource.SetMetaData(this.MetaData);
            outputSource.SetProviderID(this.ProviderID);
            outputSource.SetSourceName(this.SourceName);
        }


        public void SetMetaData(List<MetaDataObject> metaData)
        {
            Dictionary<string, MetaDataObject> dict = new Dictionary<string, MetaDataObject>();
            
            foreach(var i in metaData)
            {
                dict.Add(i.ID, i);
            }

            this.MetaData = dict;

        }
    }
}
