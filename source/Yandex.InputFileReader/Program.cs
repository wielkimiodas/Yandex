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
                            new OutputGenerator(PathResolver.OutputPath)))
            {
                opener.Read();
            }
            watch.Stop();
            int nGroups = groups.Count;
            groups.Clear();
            GC.Collect();
            Console.WriteLine(nGroups + " groups processed in " + watch.Elapsed);
        }

        private static void Main(string[] args)
        {
            if (false)
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream file = new FileStream(PathResolver.TestProcessedFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }

                InputFileOpener opener = new InputFileOpener(new BinaryReader(ms), new LinkSorter(null, null));

                return;
            }

            var watch = Stopwatch.StartNew();
            //start();
            var filename = PathResolver.TestProcessedFile+"2";
            using (var opener = new InputFileOpener(filename,
                            new OutputGenerator(PathResolver.OutputPath)))
            {
                opener.Read();
            }

            watch.Stop();
            Console.WriteLine("Done after {0}", watch.Elapsed);

            Console.ReadLine();
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

            const int N_GROUPS = 15;

            groups.Sort((o1, o2) => o2.Count - o1.Count);

            using (var writer = new StreamWriter(new FileStream(PathResolver.ClicksAnalyse, FileMode.CreateNew, FileAccess.Write)))
            {
                while (groups.Count > 0)
                {
                    if (Console.KeyAvailable)
                        if (Console.ReadKey().Key == ConsoleKey.Enter)
                            break;

                    var tmpGropus = new List<BinarySearchSet<int>>();
                    int nGroups = Math.Min(groups.Count, N_GROUPS);

                    tmpGropus.AddRange(groups.GetRange(0, nGroups));
                    groups.RemoveRange(0, nGroups);

                    Process(tmpGropus, writer);
                }
            }
        }
    }
}