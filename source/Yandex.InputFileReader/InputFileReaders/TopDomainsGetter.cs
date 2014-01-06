using System;
using System.Collections.Generic;
using System.IO;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class TopDomainsGetter : InputFileReader
    {
        private string output;
        private Dictionary<int, int> domainsCount = new Dictionary<int, int>();
        private HashSet<int> processedQueries = new HashSet<int>();
        private HashSet<int> currentDomains = new HashSet<int>();

        public TopDomainsGetter(string output)
        {
            this.output = output;
        }

        public override void Dispose()
        {
            currentDomains.Clear();
            domainsCount.Clear();
            processedQueries.Clear();
            GC.Collect();
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            if (processedQueries.Contains(queryAction.queryId))
                return;

            processedQueries.Add(queryAction.queryId);

            for (int i = queryAction.nUrls - 1; i >= 0; i--)
                currentDomains.Add(queryAction.domains[i]);

            foreach (int domain in currentDomains)
                if (domainsCount.ContainsKey(domain))
                    domainsCount[domain]++;
                else
                    domainsCount.Add(domain, 1);

            currentDomains.Clear();
        }

        public override void onEndRead()
        {
            List<Tuple<int, int>> allDomains = new List<Tuple<int, int>>();

            foreach (var element in domainsCount)
                allDomains.Add(new Tuple<int, int>(element.Key, element.Value));
            domainsCount.Clear();
            GC.Collect();

            allDomains.Sort((o1, o2) => { return o2.Item2 - o1.Item2; });

            if (allDomains.Count > 1000)
                allDomains.RemoveRange(1000, allDomains.Count - 1000);

            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (var element in allDomains)
                    writer.WriteLine(element.Item1 + "\t" + element.Item2);
            }
        }
    }
}