using System;
using System.Collections.Generic;
using System.IO;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    public class TopTermsGetter : InputFileReader
    {
        private string output;
        private Dictionary<int, int> termsCount = new Dictionary<int, int>();
        private HashSet<int> processedQueries = new HashSet<int>();
        private HashSet<int> currentTerms = new HashSet<int>();

        public TopTermsGetter(string output)
        {
            this.output = output;
        }

        public override void Dispose()
        {
            currentTerms.Clear();
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
                int term = reader.ReadInt32();

                if (process)
                    currentTerms.Add(term);
            }

            foreach (int term in currentTerms)
                if (termsCount.ContainsKey(term))
                    termsCount[term]++;
                else
                    termsCount.Add(term, 1);

            currentTerms.Clear();

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                reader.ReadInt32();

                // DOMAIN_ID
                reader.ReadInt32();
            }
        }

        public override void onEndRead()
        {
            List<Tuple<int, int>> allTerms = new List<Tuple<int, int>>();

            foreach (var element in termsCount)
                allTerms.Add(new Tuple<int, int>(element.Key, element.Value));
            termsCount.Clear();
            GC.Collect();

            allTerms.Sort((o1, o2) => { return o2.Item2 - o1.Item2; });

            if (allTerms.Count > 1000)
                allTerms.RemoveRange(1000, allTerms.Count - 1000);

            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (var element in allTerms)
                    writer.WriteLine(element.Item1 + "\t" + element.Item2);
            }
        }
    }
}