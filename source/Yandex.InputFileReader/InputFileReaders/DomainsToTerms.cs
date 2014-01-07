using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;
using System.IO;
using Yandex.Utils.UserActions;

namespace Yandex.InputFileReader
{
    public class DomainsToTerms : InputFileReader
    {
        class Comparer : IComparer<Tuple<int, BinarySearchSet<int>>>
        {
            public int Compare(Tuple<int, BinarySearchSet<int>> x, Tuple<int, BinarySearchSet<int>> y)
            {
                return x.Item1 - y.Item1;
            }
        }

        String outputFile;
        Dictionary<int, BinarySearchSet<int>> domainsTerms;
        HashSet<int> currentDomains = new HashSet<int>();
        
        public DomainsToTerms(String outputFile)
        {
            this.outputFile = outputFile;
        }

        public override void onBeginRead()
        {
            domainsTerms = new Dictionary<int, BinarySearchSet<int>>();
        }

        public override void onQueryAction(QueryAction queryAction)
        {
            for (int i = queryAction.nUrls - 1; i >= 0; i--)
            {
                if (!currentDomains.Add(queryAction.domains[i]))
                    continue;
                
                if (!domainsTerms.ContainsKey(queryAction.domains[i]))
                    domainsTerms.Add(queryAction.domains[i], new BinarySearchSet<int>(Comparer<int>.Default));

                BinarySearchSet<int> domainSet = domainsTerms[queryAction.domains[i]];

                for (int term = queryAction.nTerms - 1; term >= 0; term--)
                    domainSet.Add(queryAction.terms[term]);
            }

            currentDomains.Clear();
        }

        public override void onEndRead()
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                int counter = 0;

                foreach (var domain in domainsTerms)
                {
                    if (++counter % 10000 == 0)
                        Console.Write("Finalizing: {0} %\r", (100.0f * counter / domainsTerms.Count).ToString("0.000"));

                    writer.WriteLine("Domain {0}:", domain.Key);
                    foreach (var termId in domain.Value)
                        writer.WriteLine("{0}", termId);
                    writer.WriteLine();
                }
            }
        }
    }
}
