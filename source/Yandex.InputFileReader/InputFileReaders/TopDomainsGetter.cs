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

        public override void onQueryAction(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // TIME
            reader.ReadInt32();
            // SERPID
            reader.ReadInt32();
            // QUERYID
            int queryId = reader.ReadInt32();

            bool process = !processedQueries.Contains(queryId);
            if (process)
                processedQueries.Add(queryId);

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // TERM ID
                reader.ReadInt32();
            }

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                reader.ReadInt32();

                // DOMAIN_ID
                int domain = reader.ReadInt32();

                if (process)
                    currentDomains.Add(domain);
            }

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