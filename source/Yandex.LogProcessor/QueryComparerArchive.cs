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
        public HashSet<YandexQuery> CreateQueryVectors(HashSet<int> queries)
        {
            Console.Write("Computing queries vectors... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var yandexQueries = new HashSet<YandexQuery>();
            Semaphore s = new Semaphore(0, queries.Count);
            int waited = 0;
            int a = 0;
            HashSet<int> visited = new HashSet<int>();
            foreach (var query in queries)
            {
                visited.Add(query);
                a++;
                Int32 Q = query;
                ThreadPool.QueueUserWorkItem((WaitCallback)delegate
                {
                    YandexQuery q = new YandexQuery() { Id = Q, Vector = FindQueryInUrlsAndTerms(Q) };
                    lock (yandexQueries)
                    {
                        yandexQueries.Add(q);
                    }
                    s.Release();
                });

                if (visited.Count >= 5000000)
                {
                    for (int i = 0; i < visited.Count; i++)
                        s.WaitOne();
                    int b = 0;
                    Semaphore s2 = new Semaphore(0, TopUrlsAndTermsQueries.Length);

                    foreach (var arr in TopUrlsAndTermsQueries)
                    {
                        var array = arr;
                        ThreadPool.QueueUserWorkItem((WaitCallback)delegate
                        {
                            foreach (var v in visited)
                            {
                                if (array.Contains(v))
                                    array.Remove(v);
                            }

                            s2.Release();
                        });
                    }

                    for (int i = 0; i < TopUrlsAndTermsQueries.Length; i++)
                        s2.WaitOne();

                    waited += visited.Count;
                    visited.Clear();
                    GC.Collect();
                }

            }
            for (int i = waited; i < queries.Count; i++)
                s.WaitOne();

            stopwatch.Stop();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds + "s.");
            return yandexQueries;
        }

        private byte[] FindQueryInUrlsAndTerms(int query)
        {
            var vector = new byte[25];
            for (int i = 0; i < TopUrlsAndTermsQueries.Count(); i++)
            {
                if (TopUrlsAndTermsQueries[i].Contains(query))
                {
                    int arrayIterator = i / 8;
                    int offset = i % 8;
                    vector[arrayIterator] |= Convert.ToByte(1 << offset);
                }
            }
            return vector;
        }

        public void CompareQueriesOld(HashSet<YandexQuery> yandexQueries)
        {
            Console.Write("Comparing queries... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var writer = new StreamWriter(_outputPath);

            for (int i = 0; i < yandexQueries.Count; i++)
            {
                var obiektTomka = new List<Tuple<int, float>>();
                for (int j = 0; j < yandexQueries.Count; j++)
                {
                    if (i == j) continue;
                    var res = CompareTwoVectors(yandexQueries.ElementAt(i).Vector, yandexQueries.ElementAt(j).Vector);
                    obiektTomka.Add(new Tuple<int, float>(j, res));
                }

                obiektTomka.Sort((o1, o2) => (int)(o2.Item2 - o1.Item2));

                writer.WriteLine(i);
                for (int q = 0; q < 50; q++)
                {
                    writer.WriteLine(obiektTomka[q].Item1 + "\t" + obiektTomka[q].Item2);
                }

            }
            stopwatch.Stop();
            writer.Close();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds);
        }

        private float CompareTwoVectors(byte[] v1, byte[] v2)
        {
            var sumAnd = 0;
            var sumOr = 0;

            for (int i = 0; i < v1.Length; i++)
            {
                var bAnd = (byte)(v1[i] & v2[i]);
                var bOr = (byte)(v1[i] | v2[i]);

                while (bAnd > 0)
                {
                    while (bAnd > 0)
                    {
                        sumAnd += bAnd % 2;
                        bAnd /= 2;
                    }
                }

                while (bOr > 0)
                {
                    while (bOr > 0)
                    {
                        sumOr += bOr % 2;
                        bOr /= 2;
                    }
                }
            }

            return sumAnd / (float)sumOr;
        }
    }

    public class YandexQuery
    {
        public int Id { get; set; }
        public byte[] Vector { get; set; }
    }
}
