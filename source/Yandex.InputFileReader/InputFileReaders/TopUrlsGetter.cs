using System;
using System.Collections.Generic;
using System.IO;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class TopUrlsGetter : InputFileReader
    {
        private string output;
        private Dictionary<int, int> termsCount = new Dictionary<int, int>();
        private HashSet<int> processedQueries = new HashSet<int>();
        private HashSet<int> currentUrls = new HashSet<int>();

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

        public override void onQueryAction(QueryAction queryAction)
        {
            if (processedQueries.Contains(queryAction.queryId))
                return;

            processedQueries.Add(queryAction.queryId);

            for (int i = queryAction.nUrls - 1; i >= 0; i--)
                currentUrls.Add(queryAction.urls[i]);

            foreach (int term in currentUrls)
                if (termsCount.ContainsKey(term))
                    termsCount[term]++;
                else
                    termsCount.Add(term, 1);

            currentUrls.Clear();
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