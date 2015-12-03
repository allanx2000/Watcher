using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Jobs
{
    public class FilterUtil
    {

        internal static void FilterAndAdd(List<JobItem> pageItems, ConcurrentBag<AbstractItem> items, int pageCounter, object luk)
        {
            Thread th = new Thread(() =>
            {
                lock (luk)
                {
                    pageCounter++;
                }

                DoFilterAndAdd(pageItems, items);

                lock (luk)
                {
                    pageCounter--;
                }
            });

            th.Start();
        }

        private static readonly List<string> ExcludeKeywords = new List<string>()
        {
            "java",
            "c++",
            "support",
            "tester",
            "security",
            "consultant",
            "contract",
            "magento",
            "unity",
            "vertica",
            "adobe",
            "ruby",
            "websphere",
            "perl",
            "tableau",
            "business analyst",
            "db2",
            "etl ",
            "documentum",
            "tivoli",
            "informatica",
            "ios ",
            "oracle",
            "php",
            "qa",
            "sap ",
            "sas ",
            "sharepoint"
        };

        private static readonly List<string> IncludeKeywords = new List<string>()
        {
            "jersey city",
            "new york",
            "manhattan",
        };


        internal static void DoFilterAndAdd(List<JobItem> pageItems, ConcurrentBag<AbstractItem> items)
        {
            foreach (var i in pageItems)
            {
                string lower = i.Name.ToLower();

                bool exclude = false;

                foreach (string ex in ExcludeKeywords)
                {
                    if (lower.Contains(ex))
                    {
                        exclude = true;
                        break;
                    }
                }

                //Name has every thing
                if (!exclude)
                {
                    bool hasLocation = false;

                    foreach (string ex in IncludeKeywords)
                    {
                        if (lower.Contains(ex))
                        {
                            hasLocation = true;
                            break;
                        }
                    }

                    exclude = !hasLocation;
                }

                if (!exclude)
                    items.Add(i);
            }
        }

    }
}
