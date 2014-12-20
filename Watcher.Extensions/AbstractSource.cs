using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Watcher.Extensions
{

    public abstract class AbstractSource
    {
        public int? ID {get; private set;}
        public string ProviderID {get; private set;}
        public string SourceName { get; private set; }
        //public string URL { get; private set; }

       

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
                return MetaData.ContainsKey(URL) ? MetaData[URL] : null;
            }
        }


        private void AddProtectedMetaData(string key, string value)
        {
            MetaData[key] = value;
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
                        return Deserialize(MetaData[UPDATES_COLOR]);
                    else
                        return Colors.Black;
                }
                catch
                {
                    RemoveMetaData(UPDATES_COLOR);

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

        private Dictionary<string, string> MetaData = new Dictionary<string, string>();

        /// <summary>
        /// Add a custom metadata value to the source definition
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public AbstractSource AddMetaData(string key, string value)
        {
            if (!ProtectedValues.Contains(key))
                MetaData[key] = value;
            else throw new Exception(key + " is protected and cannot be explicitly set");

            return this;
        }

        public AbstractSource RemoveMetaData(string key)
        {
            if (MetaData.ContainsKey(key))
                MetaData.Remove(key);

            return this;
        }

        /// <summary>
        /// Gets the value from the metadata
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value or null if not found or not set</returns>
        public string GetMetaDataValue(string key)
        {
            if (MetaData.ContainsKey(key))
                return MetaData[key];
            else return null;
        }

        /// <summary>
        /// Gets the entire metadata dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string> GetMetaData()
        {
            return MetaData;
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

        public AbstractSource SetMetaData(Dictionary<string, string> metadata)
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


    }
}
