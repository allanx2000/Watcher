using Watcher.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using io = System.IO;
using Watcher.Core.Items;

namespace Watcher.DataStore.File
{
    public class FileDataStore : AbstractDataStore
    {
        private const string DataStoreType = "FileDataStore";

        private static DateTime EvictCutOff = DateTime.Today.AddDays(-5);
        
        private readonly string file;

        private List<AbstractItem> all_items = new List<AbstractItem>();
        
        private string GetItemsFile()
        {
            //TODO: Change .books to .items
            return file + ".books";
        }

        private int nextSourceId;
        private int nextItemId;

        public FileDataStore(string file) : base(DataStoreType)
        {
            
            this.file = file;

            if (!io.File.Exists(file))
                io.File.CreateText(file).Close();

            if (!io.File.Exists(GetItemsFile()))
                io.File.CreateText(GetItemsFile()).Close();           
 
        }

     
        private void SaveSources()
        {
            StreamWriter sw = new StreamWriter(file);

            sw.WriteLine(nextSourceId);

            foreach (AbstractSource s in Sources)
            {
                sw.WriteLine(Serialize(s));
            }

            sw.Close();

        }

        private void SaveItems()
        {
            StreamWriter sw = new StreamWriter(GetItemsFile());

            sw.WriteLine(nextItemId);

            foreach (AbstractItem b in Items)
            {
                sw.WriteLine(Serialize(b));
            }

            sw.Close();
        }


        private void Save()
        {
            SaveSources();

            SaveItems();
        }

        
        private const char FieldDelim = '|';
        private const char MetaDelim = '`';

        private string Serialize(AbstractItem item)
        {
            string data = String.Join(FieldDelim + "",
             item.ID.Value,
             item.SourceId,
             item.AddedDate.ToString(),
             item.Name,
             item.New.ToString(),
             item.ActionContent);

            return data;
        }

        private string Serialize(AbstractSource src)
        {
            string meta = String.Join(MetaDelim + "", 
                from kv in src.GetMetaData() select kv.Key + "=" + kv.Value);

            string data = String.Join(FieldDelim + "",
                src.ID.Value,
                src.ProviderID,
                src.SourceName,
                src.Url,
                meta);

            return data;
        }


        private AbstractItem ParseToItem(string data)
        {
            string[] fields = data.Split(FieldDelim);

            int idx = 0;

            int id = Int32.Parse(fields[idx++]);
            int srcId = Int32.Parse(fields[idx++]);
            DateTime addedDate = DateTime.Parse(fields[idx++]);
            string title = fields[idx++];
            bool isNew = Boolean.Parse(fields[idx++]);
            string url = fields[idx++];

            AbstractItem b = AbstractItem.CreateGenericItem(id, Sources.Single(x => x.ID == srcId),
                title, url, isNew, addedDate);
            
            return b;
        }

        private AbstractSource ParseToSource(string data)
        {
            string[] fields = data.Split(FieldDelim);

            int idx = 0;

            int id = Int32.Parse(fields[idx++]);
            string providerId = fields[idx++];  
            string catName = fields[idx++];
            string url = fields[idx++];
            string meta = fields[idx++];


            Dictionary<string, string> metaDict = new Dictionary<string, string>();

            foreach (string kv in meta.Split(new char[]{MetaDelim}, StringSplitOptions.RemoveEmptyEntries))
            {
                int split = kv.IndexOf('=');
                string key = kv.Substring(0, split);
                string val = kv.Substring(split + 1);

                metaDict.Add(key, val);
            }


            AbstractSource s = new GenericSource(catName, providerId);
            s.SetID(id);
            s.SetMetaData(metaDict);

            //Don't need, url

            return s;
        }

        protected override List<AbstractSource> LoadSources()
        {
            List<AbstractSource> allSources = new List<AbstractSource>();

            io.StreamReader sr = new io.StreamReader(file);

            string firstLine = sr.ReadLine();

            if (firstLine == null)
            {
                nextSourceId = 1;
            }
            else
            {
                nextSourceId = Int32.Parse(firstLine);

                while (!sr.EndOfStream)
                {
                    var src = ParseToSource(sr.ReadLine());

                    allSources.Add(src);
                }
            }

            sr.Close();
            
            return allSources;
        }

        public override void RemoveFromDataStore(AbstractSource source)
        {
            SaveSources();
        }

        protected override void DoInsertSource(AbstractSource source)
        {
            source.SetID(nextSourceId++);
            SaveSources();
        }

        protected override void DoUpdateSource(AbstractSource source)
        {
            var oldSource = Sources.First(x => x.ID == source.ID);

            source.CopyTo(oldSource);

            SaveSources();
        }

        protected override bool DoAddItem(AbstractItem item)
        {
            if (all_items.Contains(item)) return false;
            else
            {
                all_items.Add(item);

                item.SetId(nextItemId++);
                SaveItems();

                return true;
            }
        }

        protected override List<AbstractItem> LoadItems()
        {
            List<AbstractItem> items = new List<AbstractItem>();

            io.StreamReader sr = new io.StreamReader(GetItemsFile());

            string firstLine = sr.ReadLine();

            if (firstLine == null)
            {
                nextItemId = 1;
            }
            else
            {
                nextItemId = Int32.Parse(firstLine);
            }

            
                while (!sr.EndOfStream)
                {
                    var item = ParseToItem(sr.ReadLine());

                    all_items.Add(item);

                    if (item.AddedDate >= EvictCutOff)
                        items.Add(item);
                }

            sr.Close();

            return items;
        }


        public override void UpdateItem(AbstractItem item)
        {
            SaveItems();
        }
    }
}
