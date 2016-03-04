using Innouvous.Utils.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Watcher.Extensions;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.DataStore.SQLite
{
    public class SQLiteDataStore : AbstractDataStore
    {
        private SQLiteClient sqlWrapper;

        public const string DATA_STORE_TYPE = "SQLiteDataStore";

        private const string ScriptsPath = "TableScripts";

        public SQLiteDataStore(string fileName)
            : base(DATA_STORE_TYPE)
        {

            FileInfo fi = new FileInfo(fileName);

            if (fi.Exists)
            {
                sqlWrapper = new SQLiteClient(fileName, false, new Dictionary<string, string>());

                RunEviction();

                DeleteOrphanedItems();
            }
            else
            {
                sqlWrapper = new SQLiteClient(fileName, true, new Dictionary<string, string>());

                CreateNewDatabase();
            }

        }

        private void DeleteOrphanedItems()
        {
            string command = "select i.ID, s.ProviderID from tbl_items i";
            command += " left join tbl_sources s on i.SourceID = s.ID";
            command += " where s.ProviderID is null";

            List<int> ids = new List<int>();
            var results = sqlWrapper.ExecuteSelect(command);

            foreach (DataRow r in results.Rows)
            {
                ids.Add(Convert.ToInt32(r["ID"]));
            }

            command = "delete from {0} where ID in ({1})";
            command = String.Format(command, ItemsTable, String.Join(",", ids));

            sqlWrapper.ExecuteNonQuery(command);
        }

        private const int EvictionPeriod = 90; //days
        private void RunEviction()
        {
            DateTime evictionDate = DateTime.Today.AddDays(-EvictionPeriod);
            string command = "delete from {0} where AddedDate < '{1}'";
            command = String.Format(command, ItemsTable, SQLUtils.ToSQLDateTime(evictionDate));

            sqlWrapper.ExecuteNonQuery(command);
        }


        private const string SourcesTable = "tbl_sources";
        private const string ItemsTable = "tbl_items";
        private const string HashTable = "tbl_hash";

        private void CreateNewDatabase()
        {
            sqlWrapper.ExecuteNonQuery(LoadCommandFT("CreateSourcesTable", SourcesTable));
            sqlWrapper.ExecuteNonQuery(LoadCommandFT("CreateHashTable", HashTable, SourcesTable));
            sqlWrapper.ExecuteNonQuery(LoadCommandFT("CreateItemsTable", ItemsTable, SourcesTable));
        }

        private string LoadCommandFT(string fileName, params object[] args)
        {
            var result = SQLUtils.LoadCommandFromText(ScriptsPath, fileName, "txt", args);
            return result;
        }

        private Dictionary<int, ISource> sourceLookup = new Dictionary<int, ISource>();

        protected override List<GenericSource> LoadSources()
        {
            string command = "select ID, ProviderID, Name, MetaData from " + SourcesTable;

            var results = sqlWrapper.ExecuteSelect(command);

            List<GenericSource> sources = new List<GenericSource>();

            foreach (DataRow row in results.Rows)
            {
                GenericSource source = new GenericSource(row["Name"].ToString(), row["ProviderID"].ToString(), false); //TODO: new function
                source.SetID(Int32.Parse(row["ID"].ToString()));
                source.SetMetaData(DeserializeMeta(row["MetaData"].ToString()));

                sourceLookup.Add(source.ID.Value, source);

                sources.Add(source);
            }

            return sources;
        }


        private readonly DateTime LatestCutOff = DateTime.Today.AddDays(-5);

        protected override List<IDataItem> LoadItems()
        {
            
            string evictString = SQLUtils.ToSQLDateTime(LatestCutOff);

            string command = LoadCommandFT("SelectDefaultItems", ItemsTable, evictString);

            DataTable results = sqlWrapper.ExecuteSelect(command);

            List<IDataItem> items = LoadItemsFromTable(results);


            return items;
        }

        private List<IDataItem> LoadItemsFromTable(DataTable results)
        {
            List<IDataItem> items = new List<IDataItem>();

            foreach (DataRow r in results.Rows)
            {
                try
                {
                    int id = Convert.ToInt32(r["ID"]);
                    string name = r["Name"].ToString();
                    int srcId = Convert.ToInt32(r["SourceID"]);
                    DateTime addedDate = Convert.ToDateTime(r["AddedDate"]);
                    bool isNew = Convert.ToBoolean(r["IsNew"]);
                    string actionContent = r["ActionContent"].ToString();

                    if (sourceLookup.ContainsKey(srcId))
                    {
                        IDataItem i = AbstractItem.CreateGenericItem(id, sourceLookup[srcId],
                            name, actionContent, isNew, addedDate);

                        items.Add(i);
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);

                    throw e;
                }
            }

            return items;
        }

        protected override void RemoveFromDataStore(ISource source)
        {
            int id = source.ID.Value;
            string command;

            //Delete items
            command = "delete from {0} where SourceId={1}";
            command = String.Format(command, ItemsTable, id);
            sqlWrapper.ExecuteNonQuery(command);
            
            //Delete Source
            command = LoadCommandFT("DeleteID", SourcesTable, id);
            sqlWrapper.ExecuteNonQuery(command);
            
        }

        protected override void DoInsertSource(ISource source)
        {
            string command = LoadCommandFT("InsertSource", SourcesTable, source.ProviderID, source.SourceName, SerializeMeta(source.GetMetaData()));

            sqlWrapper.ExecuteNonQuery(command);

            source.SetID(GetLastInsertRow());
        }

        private int GetLastInsertRow()
        {
        /*    string command = "select last_insert_rowid()";

            var result = sqlWrapper.ExecuteScalar(command);

            return Int32.Parse(result.ToString());*/

            return SQLUtils.GetLastInsertRow(sqlWrapper);
        }

        private const char MetaDelim = '|';


        private string SerializeMeta(Dictionary<string, IMetaDataObject> dictionary)
        {
            throw new NotImplementedException();
        }

        private string SerializeMeta(Dictionary<string, string> dictionary)
        {
            string meta = String.Join(MetaDelim.ToString(), from kv in dictionary select kv.Key + "=" + kv.Value);

            return meta;
        }

        //TODO: Need to make for new M<eta style...
        private Dictionary<string, string> DeserializeMeta(string meta)
        {
            string[] data = meta.Split(MetaDelim);

            Dictionary<string, string> kvs = new Dictionary<string, string>();

            foreach (string kv in data)
            {
                int split = kv.IndexOf('=');
                string key = kv.Substring(0, split);
                string value = kv.Substring(split+1);

                kvs.Add(key, value);
            }

            return kvs;
        }

        protected override void DoUpdateSource(ISource source)
        {
            string command;

            if (source.ID != null)
                command = LoadCommandFT("UpdateSource", SourcesTable, source.SourceName, SerializeMeta(source.GetMetaData()),
                    source.ID.Value);
            else
                command = LoadCommandFT("InsertSource", SourcesTable, source.ProviderID, source.SourceName, SerializeMeta(source.GetMetaData()));

            sqlWrapper.ExecuteNonQuery(command);

            if (source.ID == null)
                source.SetID(GetLastInsertRow());
        }

        protected override bool DoAddItem(IDataItem item)
        {
            string command = "select ID from {0} WHERE Name='{1}'";

            string name = item.Name;

            if (name.Contains("'"))
                name = name.Replace("'", "''");

            command = String.Format(command, ItemsTable, name);

            bool shouldAdd = sqlWrapper.ExecuteSelect(command).Rows.Count == 0;

            if (shouldAdd)
            {
                command = LoadCommandFT("InsertItem", ItemsTable, item.Name, item.SourceId, SQLUtils.ToSQLDateTime(item.AddedDate), 1, item.ActionContent);

                sqlWrapper.ExecuteNonQuery(command);

                item.SetId(GetLastInsertRow());

                return true;
            }
            else return false;
        }


     

        public override void UpdateItem(IDataItem item)
        {
            string command = LoadCommandFT("UpdateItem", ItemsTable, item.New ? 1 : 0, item.ID.Value);

            sqlWrapper.ExecuteNonQuery(command);


        }

        public override List<IDataItem> Search(string filter)
        {
            string sql = "select * from {0} where Name LIKE '%{1}%'";
            sql = String.Format(sql, ItemsTable, filter);

            return LoadItemsFromTable(sqlWrapper.ExecuteSelect(sql));
        }
    }
}
