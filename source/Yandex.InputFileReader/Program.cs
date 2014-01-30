using System;
using System.Diagnostics;
using Yandex.InputFileReader.InputFileReaders;
using Yandex.Utils;
using System.Collections.Generic;
using System.IO;

namespace Yandex.InputFileReader
{
    internal class Program
    {
        private static void Process(List<BinarySearchSet<int>> groups, StreamWriter writer)
        {
            var watch = Stopwatch.StartNew();
            var filename = PathResolver.TrainProcessedFile;
            using (var opener = new InputFileOpener(filename,
                            new LinkSorter(groups, writer)))
            {
                opener.Read();
            }
            watch.Stop();
            groups.Clear();
            GC.Collect();
            Console.WriteLine(groups.Count + " groups processed in " + watch.Elapsed);
        }

        private static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            start();
            watch.Stop();
            Console.WriteLine("Done after {0}", watch.Elapsed);
        }

        private static void start()
        {
            var groups = new List<BinarySearchSet<int>>();

            using (var r = new StreamReader(PathResolver.UsersGroups))
            {
                var watch = Stopwatch.StartNew();

                
                int groupsCount = 0;

                while(r.Peek() > -1)
                {
                    String line;
                    var group = new List<int>();
                    while (!String.IsNullOrEmpty(line = r.ReadLine()))
                        group.Add(Int32.Parse(line));

                    groups.Add(new BinarySearchSet<int>(group, Comparer<int>.Default));
                    groupsCount++;
                }

                watch.Stop();
                Console.WriteLine("Reading {0} groups took {1}", groupsCount, watch.Elapsed);
            }

            const int N = 12;
            groups.Sort((o1, o2) => o2.Count - o1.Count);
            if (groups.Count > 2 * N)
                groups.RemoveRange(2 * N, groups.Count - 2 * N);

            using (var writer = new StreamWriter(PathResolver.ClicksAnalyse))
            {
                while (groups.Count > 0)
                {
                    var tmpGropus = new List<BinarySearchSet<int>>();
                    int nGroups = Math.Min(groups.Count, N);

                    tmpGropus.AddRange(groups.GetRange(0, nGroups));
                    groups.RemoveRange(0, nGroups);

                    Process(tmpGropus, writer);
                }
            }

            Console.ReadLine();
        }
    }
}