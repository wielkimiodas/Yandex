using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Yandex.LogProcessor
{
    public partial class QueryComparer
    {
        private readonly string _path = @"C:\Users\Wojciech\Desktop\log.txt";
        private readonly string _outputPath = @"C:\Users\Wojciech\Desktop\out.txt";
        private List<int>[] TopUrlsAndTermsQueries;

        public QueryComparer()
        {
        }

        public QueryComparer(string path, string outputPath)
        {
            this._path = path;
            this._outputPath = outputPath;
        }

        public HashSet<int> ReadQueryList()
        {
            TopUrlsAndTermsQueries = new List<int>[200];
            var textReader = new StreamReader(_path);
            var queries = new HashSet<int>();
            var globalStopwatch = new Stopwatch();
            var segmentStopwatch = new Stopwatch();

            for (int i = 0; i < 200; i++)
            {
                TopUrlsAndTermsQueries[i] = new List<int>();
            }

            //******************
            //* urls segment   *
            //******************

            Console.Write("Processing urls... ");
            globalStopwatch.Start();
            segmentStopwatch.Start();

            //eliminate count info
            var line = textReader.ReadLine();
            while (!line.Equals(""))
            { line = textReader.ReadLine(); }

            //read url queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (!tmp.Equals(""))
                {
                    TopUrlsAndTermsQueries[i].Add(Convert.ToInt32(tmp));
                    queries.Add(Convert.ToInt32(tmp));
                    tmp = textReader.ReadLine();
                }
            }
            segmentStopwatch.Stop();
            Console.WriteLine("took " + segmentStopwatch.Elapsed.TotalSeconds + "s.");
            Console.WriteLine("Url queries count: " + queries.Count);


            //******************
            //* terms segment  *
            //******************

            Console.Write("Processing terms... ");
            segmentStopwatch.Restart();

            //eliminate count info
            line = textReader.ReadLine();
            while (!line.Equals(""))
            { line = textReader.ReadLine(); }

            //read term queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (!tmp.Equals(""))
                {
                    TopUrlsAndTermsQueries[i + 100].Add(Convert.ToInt32(tmp));
                    queries.Add(Convert.ToInt32(tmp));
                    tmp = textReader.ReadLine();
                }
            }

            segmentStopwatch.Stop();
            globalStopwatch.Stop();
            textReader.Close();
            Console.WriteLine("took " + segmentStopwatch.Elapsed.TotalSeconds + "s.");
            Console.WriteLine("Total count: " + queries.Count);
            Console.WriteLine("Total time: " + globalStopwatch.Elapsed.TotalSeconds + "s.");

            return queries;
        }

        public Dictionary<int, List<int>> CreateQueryLists(HashSet<int> queries)
        {
            Console.Write("Computing queries lists... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var queriesWithUrlsTermsMapped = new Dictionary<int, List<int>>();

            //foreach top 100 url and top 100 term
            for (int i = 0; i < TopUrlsAndTermsQueries.Length; i++)
            {
                //foreach query which occured in current url/term
                for (int j = 0; j < TopUrlsAndTermsQueries[i].Count; j++)
                {
                    var query = TopUrlsAndTermsQueries[i][j];
                    if (!queriesWithUrlsTermsMapped.ContainsKey(query))
                    {
                        queriesWithUrlsTermsMapped.Add(query, new List<int>());
                    }
                    queriesWithUrlsTermsMapped[query].Add(i);
                }
            }

            foreach (var query in queriesWithUrlsTermsMapped)
            {
                query.Value.Sort();
            }

            stopwatch.Stop();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds + "s.");
            return queriesWithUrlsTermsMapped;
        }

        private static float CompareTwoLists(List<int> list1, List<int> list2)
        {
            int i = 0, j = 0;
            int sum = 0;
            int all = 0;
            while (i < list1.Count && j < list2.Count)
            {
                if (list1[i] != list2[j])
                {
                    if (list1[i] < list2[j])
                        i++;
                    else
                        j++;
                    all++;
                }
                else
                {
                    sum += 2;
                    all += 2;
                    i++;
                    j++;
                }
            }
            all += list1.Count - i;
            all += list2.Count - j;

            return sum / (float)all;
        }

        public void CompareQueries(Dictionary<int, List<int>> queries)
        {
            Console.Write("Comparing queries... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var writer = new StreamWriter(_outputPath);

            int count = 0;
            foreach (var q1 in queries)
            {
                foreach (var q2 in queries)
                {
                    if (q1.Key == q2.Key)
                        continue;
                    var res = CompareTwoLists(q1.Value, q2.Value);
                    writer.WriteLine(q1.Key + "\t" + res);
                }
            }

            stopwatch.Stop();
            writer.Close();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds + "s.");
        }
    }
}
