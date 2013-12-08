using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class TopUrlsGetter : InputFileReader
    {
        string output;
        Dictionary<int, int> termsCount = new Dictionary<int, int>();
        HashSet<int> processedQueries = new HashSet<int>();
        HashSet<int> currentUrls = new HashSet<int>();

        public TopUrlsGetter(string output)
        {
            this.output = output;
        }

        public override void Dispose()
        {
            currentUrls.Clear();
            termsCount.Clear();
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

            foreach(int term in currentUrls)
                if (termsCount.ContainsKey(term))
                    termsCount[term]++;
                else
                    termsCount.Add(term, 1);

            currentUrls.Clear();

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                int url = reader.ReadInt32();

                // DOMAIN_ID
                reader.ReadInt32();

                if (process)
                    currentUrls.Add(url);
            }
        }

        public override void onEndRead()
        {
            List<Tuple<int, int>> allUrls = new List<Tuple<int, int>>();

            foreach (var element in termsCount)
                allUrls.Add(new Tuple<int, int>(element.Key, element.Value));
            termsCount.Clear();
            GC.Collect();

            allUrls.Sort((o1, o2) => { return o2.Item2 - o1.Item2; });

            if (allUrls.Count > 1000)
                allUrls.RemoveRange(1000, allUrls.Count - 1000);

            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (var element in allUrls)
                    writer.WriteLine(element.Item1 + "\t" + element.Item2);
            }
        }
    }
}
