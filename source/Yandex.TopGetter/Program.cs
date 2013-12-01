using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Yandex.TopGetter
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = @"D:\Downloads\EDWD\";
            string data = "train";

            string logFile = dir + data + "_tr";
            string topUrls = dir + data + "_top_urls.txt";
            string topTerms = dir + data + "_top_terms.txt";
            string outputFile = dir + data + "_output.txt";

            Stopwatch watch1 = new Stopwatch();
            Stopwatch watch2 = new Stopwatch();
            watch1.Start();
            watch2.Start();

            getTopUrls(logFile, topUrls);
            watch1.Stop();
            Console.WriteLine("getTopUrls: " + watch1.ElapsedMilliseconds);
            watch1.Restart();

            getTopTerms(logFile, topTerms);
            watch1.Stop();
            Console.WriteLine("getTopTerms: " + watch1.ElapsedMilliseconds);
            watch1.Restart();

            using (StreamWriter output = new StreamWriter(outputFile))
            {
                getUrlsFromTop(logFile, topUrls, output);
                watch1.Stop();
                Console.WriteLine("getUrlsFromTop: " + watch1.ElapsedMilliseconds);
                watch1.Restart();

                getTermsFromTop(logFile, topTerms, output);
                watch1.Stop();
                Console.WriteLine("getUrlsFromTop: " + watch1.ElapsedMilliseconds);
            }

            watch2.Stop();
            Console.WriteLine("Total: " + watch2.ElapsedMilliseconds);
            Console.ReadLine();
        }

        static void getTopUrls(string input, string output)
        {
            List<Tuple<int, int>> allUrls = new List<Tuple<int, int>>();

            Dictionary<int, int> urlsCount = new Dictionary<int, int>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (BufferedBinaryReader reader = new BufferedBinaryReader(input))
            {
                HashSet<int> processedQueries = new HashSet<int>();

                while (reader.PeekChar() > -1)
                {
                    byte type = reader.ReadByte();

                    int session = reader.ReadInt32();

                    switch (type)
                    {
                        case 0:
                            // DAY
                            reader.ReadInt32();
                            int user = reader.ReadInt32();
                            break;
                        case 1:
                        case 2:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // QUERYID
                                int query_id = reader.ReadInt32();
                                bool process = !processedQueries.Contains(query_id);
                                if (process)
                                    processedQueries.Add(query_id);

                                // TERMS
                                int termsN = reader.ReadInt32();
                                for (int i = termsN; i > 0; i--)
                                {
                                    int term = reader.ReadInt32();
                                }
                                // URLS & DOMAINS
                                for (int i = reader.ReadInt32(); i > 0; i--)
                                {
                                    int url = reader.ReadInt32();
                                    int domain = reader.ReadInt32();
                                    if (!process)
                                        continue;

                                    if (urlsCount.ContainsKey(url))
                                        urlsCount[url]++;
                                    else
                                        urlsCount.Add(url, 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // URLS
                                int url = reader.ReadInt32();
                                break;
                            }
                    }
                }
            }

            // przeniesienie z mapy do listy
            foreach (var element in urlsCount)
                allUrls.Add(new Tuple<int, int>(element.Key, element.Value));
            urlsCount.Clear();
            urlsCount = new Dictionary<int, int>();
            GC.Collect();

            watch.Stop();
            Console.WriteLine("... zakończono po {0} ({1})", watch.ElapsedMilliseconds, allUrls.Count);
            watch.Restart();

            // sortowanie po malejącej liczbie wystąpień
            allUrls.Sort((o1, o2) => { return o2.Item2 - o1.Item2; });

            watch.Stop();
            Console.WriteLine("Zakończono sortowanie danych po " + watch.ElapsedMilliseconds);
            watch.Restart();

            // zostawienie top 1000 urli
            if (allUrls.Count > 1000)
                allUrls.RemoveRange(1000, allUrls.Count - 1000);

            watch.Stop();
            Console.WriteLine("Wyczyszczono liste po " + watch.ElapsedMilliseconds);
            watch.Restart();

            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (var element in allUrls)
                {
                    writer.WriteLine(element.Item1 + "\t" + element.Item2);
                }
            }

            watch.Stop();
            Console.WriteLine("Zakończono zapisywanie danych po " + watch.ElapsedMilliseconds);
        }

        static void getTopTerms(string input, string output)
        {
            List<Tuple<int, int>> allTerms = new List<Tuple<int, int>>();

            Dictionary<int, int> termsCount = new Dictionary<int, int>();

            HashSet<int> currentTerms = new HashSet<int>();
            HashSet<int> processedQueries = new HashSet<int>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (BufferedBinaryReader reader = new BufferedBinaryReader(input))
            {
                while (reader.PeekChar() > -1)
                {
                    byte type = reader.ReadByte();

                    int session = reader.ReadInt32();

                    switch (type)
                    {
                        case 0:
                            // DAY
                            reader.ReadInt32();
                            int user = reader.ReadInt32();
                            break;
                        case 1:
                        case 2:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // QUERYID
                                int query_id = reader.ReadInt32();
                                bool process = !processedQueries.Contains(query_id);
                                if (process)
                                    processedQueries.Add(query_id);

                                // TERMS
                                int termsN = reader.ReadInt32();
                                for (int i = termsN; i > 0; i--)
                                {
                                    int term = reader.ReadInt32();
                                    if (process)
                                        currentTerms.Add(term);
                                }
                                // URLS & DOMAINS
                                for (int i = reader.ReadInt32(); i > 0; i--)
                                {
                                    int url = reader.ReadInt32();
                                    int domain = reader.ReadInt32();
                                }

                                foreach (int term in currentTerms)
                                {
                                    if (termsCount.ContainsKey(term))
                                        termsCount[term]++;
                                    else
                                        termsCount.Add(term, 1);
                                }

                                currentTerms.Clear();
                                break;
                            }
                        case 3:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // URLS
                                int url = reader.ReadInt32();
                                break;
                            }
                    }
                }
            }

            processedQueries.Clear();

            // przeniesienie z mapy do listy
            foreach (var element in termsCount)
                allTerms.Add(new Tuple<int, int>(element.Key, element.Value));
            termsCount.Clear();
            termsCount = new Dictionary<int, int>();
            GC.Collect();

            watch.Stop();
            Console.WriteLine("... zakończono po {0} ({1})", watch.ElapsedMilliseconds, allTerms.Count);
            watch.Restart();

            // sortowanie po malejącej liczbie wystąpień
            allTerms.Sort((o1, o2) => { return o2.Item2 - o1.Item2; });

            watch.Stop();
            Console.WriteLine("Zakończono sortowanie danych po " + watch.ElapsedMilliseconds);
            watch.Restart();

            // zostawienie top 1000 termów
            if (allTerms.Count > 1000)
                allTerms.RemoveRange(1000, allTerms.Count - 1000);

            watch.Stop();
            Console.WriteLine("Wyczyszczono liste po " + watch.ElapsedMilliseconds);
            watch.Restart();

            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (var element in allTerms)
                {
                    writer.WriteLine(element.Item1 + "\t" + element.Item2);
                }
            }

            watch.Stop();
            Console.WriteLine("Zakończono zapisywanie danych po " + watch.ElapsedMilliseconds);
        }

        static void getUrlsFromTop(string logFilename, string topFilename, StreamWriter output)
        {
            const int N = 100;
            List<Tuple<int, int>> urls = new List<Tuple<int, int>>();
            List<int>[] queries = new List<int>[N];
            for (int i = 0; i < N; i++)
                queries[i] = new List<int>();

            using (StreamReader reader = new StreamReader(topFilename))
            {
                string[] seps = new string[] { "\t" };

                for (int i = 0; i < N; i++)
                {
                    string line = reader.ReadLine();
                    string[] array = line.Split(seps, StringSplitOptions.None);
                    urls.Add(new Tuple<int, int>(Int32.Parse(array[0]), Int32.Parse(array[1])));
                }
            }

            List<int> sortedUrls = new List<int>();
            foreach (var url in urls)
                sortedUrls.Add(url.Item1);

            sortedUrls.Sort();

            using (BufferedBinaryReader reader = new BufferedBinaryReader(logFilename))
            {
                HashSet<int> processedQueries = new HashSet<int>();

                while (reader.PeekChar() > -1)
                {
                    byte type = reader.ReadByte();

                    int session = reader.ReadInt32();

                    switch (type)
                    {
                        case 0:
                            // DAY
                            reader.ReadInt32();
                            // USER
                            reader.ReadInt32();
                            break;
                        case 1:
                        case 2:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // QUERYID
                                int query_id = reader.ReadInt32();

                                bool process = !processedQueries.Contains(query_id);
                                if (process)
                                    processedQueries.Add(query_id);
                                // TERMS
                                int termsN = reader.ReadInt32();
                                for (int i = termsN; i > 0; i--)
                                {
                                    int term = reader.ReadInt32();
                                }
                                // URLS & DOMAINS
                                int n = reader.ReadInt32();
                                for (int i = n; i > 0; i--)
                                {
                                    int url = reader.ReadInt32();
                                    int domain = reader.ReadInt32();
                                    if (!process)
                                        continue;

                                    for (int index = 0; index < sortedUrls.Count; index++)
                                    {
                                        if (sortedUrls[index] > url)
                                            break;

                                        if (sortedUrls[index] == url)
                                            queries[index].Add(query_id);
                                    }
                                }
                                break;
                            }
                        case 3:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // URLS
                                int url = reader.ReadInt32();
                                break;
                            }
                    }
                }
            }

            {
                output.WriteLine("Top " + urls.Count + " urls:");
                for (int i = 0; i < urls.Count; i++)
                {
                    output.WriteLine(urls[i].Item1 + "\t" + urls[i].Item2);
                }

                for (int i = 0; i < urls.Count; i++)
                {
                    output.WriteLine();
                    int index = sortedUrls.IndexOf(urls[i].Item1);

                    queries[index].Sort();

                    output.WriteLine(sortedUrls[index] + ":");
                    foreach (var query in queries[index])
                    {
                        output.WriteLine(query);
                    }
                }

                output.WriteLine();
            }
        }

        static void getTermsFromTop(string logFilename, string topFilename, StreamWriter output)
        {
            const int N = 100;
            List<Tuple<int, int>> terms = new List<Tuple<int, int>>();
            List<int>[] queries = new List<int>[N];
            HashSet<int> processedQueries = new HashSet<int>();
            for (int i = 0; i < N; i++)
                queries[i] = new List<int>();

            using (StreamReader reader = new StreamReader(topFilename))
            {
                string[] seps = new string[] { "\t" };

                for (int i = 0; i < N; i++)
                {
                    string line = reader.ReadLine();
                    string[] array = line.Split(seps, StringSplitOptions.None);
                    terms.Add(new Tuple<int, int>(Int32.Parse(array[0]), Int32.Parse(array[1])));
                }
            }

            List<int> sortedTerms = new List<int>();
            foreach (var term in terms)
                sortedTerms.Add(term.Item1);

            sortedTerms.Sort();

            using (BufferedBinaryReader reader = new BufferedBinaryReader(logFilename))
            {
                HashSet<int> procesedQueries = new HashSet<int>();

                while (reader.PeekChar() > -1)
                {
                    byte type = reader.ReadByte();

                    int session = reader.ReadInt32();

                    switch (type)
                    {
                        case 0:
                            // DAY
                            reader.ReadInt32();
                            // USER
                            reader.ReadInt32();
                            break;
                        case 1:
                        case 2:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // QUERYID
                                int query_id = reader.ReadInt32();

                                bool process = !processedQueries.Contains(query_id);
                                if (process)
                                    processedQueries.Add(query_id);
                                // TERMS
                                int termsN = reader.ReadInt32();
                                for (int i = termsN; i > 0; i--)
                                {
                                    int term = reader.ReadInt32();
                                    if (!process)
                                        continue;

                                    for (int index = 0; index < sortedTerms.Count; index++)
                                    {
                                        if (sortedTerms[index] > term)
                                            break;

                                        if (sortedTerms[index] == term)
                                            queries[index].Add(query_id);
                                    }
                                }
                                // URLS & DOMAINS
                                int n = reader.ReadInt32();
                                for (int i = n; i > 0; i--)
                                {
                                    int url = reader.ReadInt32();
                                    int domain = reader.ReadInt32();
                                }
                                break;
                            }
                        case 3:
                            {
                                // TIME
                                reader.ReadInt32();
                                // SERPID
                                reader.ReadInt32();
                                // URLS
                                int url = reader.ReadInt32();
                                break;
                            }
                    }
                }
            }

            {
                output.WriteLine("Top " + terms.Count + " terms:");
                for (int i = 0; i < terms.Count; i++)
                {
                    output.WriteLine(terms[i].Item1 + "\t" + terms[i].Item2);
                }

                for (int i = 0; i < terms.Count; i++)
                {
                    output.WriteLine();
                    int index = sortedTerms.IndexOf(terms[i].Item1);

                    queries[index].Sort();

                    output.WriteLine(sortedTerms[index] + ":");
                    foreach (var query in queries[index])
                    {
                        output.WriteLine(query);
                    }
                }

                output.WriteLine();
            }
        }
    }
}
