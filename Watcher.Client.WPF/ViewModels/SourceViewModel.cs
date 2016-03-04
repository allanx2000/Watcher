using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Watcher.Core;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Client.WPF.ViewModels
{
    public class SourceViewModel : ViewModel<ISource>
    {

        public static readonly Color DefaultColor = Colors.Black;

        public const string UPDATES_COLOR = "UpdatesColor";
        public const string URL = "Url";

        private static readonly List<string> ProtectedValues = new List<string>()
        {
            UPDATES_COLOR,
            URL
        };

        public static string SerializeColor(Color color)
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
                    var color = Data.GetMetaDataValue(UPDATES_COLOR);
                    if (color != null)
                        return DeserializeColor(color.ToString());
                    else
                        return Colors.Black;
                }
                catch
                {
                    Data.ClearMetaDataValue(UPDATES_COLOR);

                    return Colors.Black;
                }
            }
        }

        public void SetUrl(string url)
        {
            var valid = true;

            if (valid)
            {
                Data.SetMetaDataValue(URL, url);
            }
            else throw new Exception("Not a valid URL");

        }

        public void SetUpdatesColor(Color color)
        {
            Data.SetMetaDataValue(UPDATES_COLOR, SerializeColor(color));            
        }


        public static Color DeserializeColor(string p)
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

        
        public string DisplayName
        {
            get
            {
                return Data.GetDisplayName();
            }
        }
        
        public string SourceName
        {
            get
            {
                return Data.SourceName;
            }
        }

        public static Color GetColor(ISource source)
        {

            object value = source.GetMetaDataValue(SourceViewModel.UPDATES_COLOR);
            if (value == null)
                return DefaultColor;
            else
                return DeserializeColor(value.ToString());
        }

        public static string GetUrl(ISource source)
        {
            object value = source.GetMetaDataValue(SourceViewModel.UPDATES_COLOR);
            if (value == null)
                return null;
            else
                return (string) value;
        }


        public SourceViewModel(ISource source) : base(source)
        {   
        }

        public override void RefreshViewModel()
        {
            base.RefreshViewModel();

        }

    }
}
