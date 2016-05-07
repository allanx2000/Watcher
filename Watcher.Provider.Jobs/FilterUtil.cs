using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Provider.Jobs
{
    //TODO: Move to Watcher core? Or pass in via an interface for upstream Watcher.GUI
    public class FilterUtil
    {
        /*
        private static List<Thread> runningThreads = new List<Thread>();

        public static IEnumerable<Thread> RunningThreads
        {
            get
            {
                var running = runningThreads.Where(x => x.ThreadState == ThreadState.Running).ToList();

                List<Thread> dead = new List<Thread>();
                foreach (var t in runningThreads)
                {
                    if (!running.Contains(t))
                        dead.Add(t);
                }

                foreach (var t in dead)
                    runningThreads.Remove(t);

                return running;
            }
        }
        */

        internal static void FilterAndAdd(List<JobItem> pageItems, ConcurrentBag<IDataItem> items, int pageCounter, object luk, ServiceProvider sp)
        {
            Thread th = new Thread(() =>
            {
                lock (luk)
                {
                    pageCounter++;
                }

                DoFilterAndAdd(pageItems, items, sp);

                lock (luk)
                {
                    pageCounter--;
                }

            });

            th.IsBackground = true;
            th.Start();
        }

        private static readonly List<string> ExcludeKeywords = new List<string>()
        {
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

        private static ConcurrentBag<string> cache = new ConcurrentBag<string>();
        internal static void DoFilterAndAdd(List<JobItem> pageItems, ConcurrentBag<IDataItem> items, ServiceProvider sp)
        {
            WebClient wc = new WebClient();

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

                if (exclude)
                    continue;

                lock (cache)
                {
                    if (cache.Contains(i.ActionContent))
                    {
                        continue;
                    }
                    else
                        cache.Add(i.ActionContent);
                }

                if (NotContract(i, wc))
                    items.Add(i);
            }
        }

        private static readonly List<string> Contracts = new List<string>() { "CON", "C2H", "Contract" };

        private static bool NotContract(JobItem i, WebClient wc)
        {
            try
            {
                string page = wc.DownloadString(i.ActionContent);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                var nodes = doc.DocumentNode.SelectNodes("//div[@class='iconsiblings']");
                foreach (var node in nodes)
                {
                    string content = node.InnerText;

                    foreach (string m in Contracts)
                    {
                        if (content.Contains(m))
                            return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
