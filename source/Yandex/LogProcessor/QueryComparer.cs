using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogProcessor
{
    public class YandexQuery
    {
        public int Id { get; set; }
        public byte[] Vector { get; set; }
    }

    public class QueryComparer
    {
        private readonly string _path = @"C:\Users\Wojciech\Desktop\log.txt";

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
            var textReader = new StreamReader(_path);
            var queries = new HashSet<int>();
            var globalStopwatch = new Stopwatch();
            var segmentStopwatch = new Stopwatch();

            //******************
            //* urls segment   *
            //******************

            Console.Write("Processing urls... ");
            globalStopwatch.Start();
            segmentStopwatch.Start();

            //eliminate count info
            var line = textReader.ReadLine();
            while(!line.Equals(""))
            { line = textReader.ReadLine(); }

            //read url queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (!tmp.Equals(""))
                {
                    queries.Add(Convert.ToInt32(tmp));
                    tmp = textReader.ReadLine();
                }
            }
            segmentStopwatch.Stop();
            Console.WriteLine("took "+ segmentStopwatch.Elapsed.TotalSeconds + "s.");
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
                    queries.Add(Convert.ToInt32(tmp));
                    tmp = textReader.ReadLine();
                }
            }

            segmentStopwatch.Stop();
            globalStopwatch.Stop();
            textReader.Close();
            Console.WriteLine("took " + segmentStopwatch.Elapsed.TotalSeconds + "s.");
            Console.WriteLine("Total count: "+ queries.Count);
            Console.WriteLine("Total time: " + globalStopwatch.Elapsed.TotalSeconds + "s.");

            return queries;
        }

        public HashSet<YandexQuery> CreateQueryVectors(HashSet<int> queries)
        {
            Console.Write("Computing queries vectors... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var yandexQueries= new HashSet<YandexQuery>();
            foreach (var query in queries)
            {
                yandexQueries.Add(new YandexQuery() {Id = query, Vector = FindQueryInUrlsAndTerms(query)});
            }
            stopwatch.Stop();
            Console.WriteLine("took "+stopwatch.Elapsed.TotalSeconds + "s.");
            return yandexQueries;
        }

        private byte[] FindQueryInUrlsAndTerms(int query)
        {
            var textReader = new StreamReader(_path);
            var vector = new byte[25];

            //******************
            //* urls segment   *
            //******************

            //eliminate count info
            var line = textReader.ReadLine();
            while (!line.Equals(""))
            { line = textReader.ReadLine(); }

            //read url queries
            for (int i = 0; i < 100; i++)
            {
                //get url id
                //var description = textReader.ReadLine();
                //var leftSide = description.Remove(description.IndexOf(':'));
                //var id = leftSide.Remove(0, leftSide.LastIndexOf(' '));

                //ignore description
                var debug = textReader.ReadLine();
                
                var tmp = textReader.ReadLine();
                while (!tmp.Equals(""))
                {
                    var curr = Convert.ToInt32(tmp);

                    //end finding in this block - only bigger id queries left
                    if (curr > query)
                    {
                        while (!textReader.ReadLine().Equals(""))
                        {
                        }
                        break;
                    }

                    if (curr == query)
                    {
                        int arrayIterator = i/8;
                        int offset = i%8;
                        vector[arrayIterator] |= Convert.ToByte(1 << offset);
                    }

                    tmp = textReader.ReadLine();
                }
            }

            //******************
            //* terms segment  *
            //******************

            //eliminate count info
            line = textReader.ReadLine();
            while (!line.Equals(""))
            { line = textReader.ReadLine(); }

            //read term queries
            for (int i = 0; i < 100; i++)
            {
                //ignore description
                var debug= textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (!tmp.Equals(""))
                {
                    var curr = Convert.ToInt32(tmp);

                    //end finding in this block - only bigger id queries left
                    if (curr > query)
                    {
                        while (!textReader.ReadLine().Equals(""))
                        {
                        }
                        break;
                    }

                    if (curr == query)
                    {
                        int arrayIterator = (i+100) / 8;
                        int offset = (i+100) % 8;
                        vector[arrayIterator] |= Convert.ToByte(1 << offset);
                    }
                    tmp = textReader.ReadLine();
                    
                }
            }

            textReader.Close();

            return vector;
        }
    }
}
