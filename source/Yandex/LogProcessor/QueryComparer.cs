﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LogProcessor
{
    public class YandexQuery
    {
        public int Id { get; set; }
        public byte[] Vector { get; set; }
    }

    public class QueryComparer
    {
        private readonly string _path = @"C:\tmp1\log2.txt";
        private HashSet<int>[] SetArray;

        public QueryComparer()
        {
        }

        public QueryComparer(string path)
        {
            this._path = path;
        }

        //C:\Users\Wojciech\Desktop\log.txt C:\Users\Wojciech\Desktop\log2\log.txt        
        public HashSet<int> ReadQueryList()
        {
            SetArray = new HashSet<int>[200];
            var textReader = new StreamReader(_path);
            var queries = new HashSet<int>();
            var globalStopwatch = new Stopwatch();
            var segmentStopwatch = new Stopwatch();

            for (int i = 0; i < 200; i++)
            {
                SetArray[i] = new HashSet<int>();
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
                    SetArray[i].Add(Convert.ToInt32(tmp));
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
                    SetArray[i + 100].Add(Convert.ToInt32(tmp));
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
                    Semaphore s2 = new Semaphore(0, SetArray.Length);

                    foreach (var arr in SetArray)
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

                    for (int i = 0; i < SetArray.Length; i++)
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
            for (int i = 0; i < SetArray.Count(); i++)
            {
                if (SetArray[i].Contains(query))
                {
                    int arrayIterator = i / 8;
                    int offset = i % 8;
                    vector[arrayIterator] |= Convert.ToByte(1 << offset);
                }
            }
            return vector;
        }
    }
}
