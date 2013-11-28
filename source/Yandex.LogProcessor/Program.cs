using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Yandex.LogProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            QueryComparer queryComparer;
            if (args.Count() == 2 && File.Exists(args[0]))
            {
                queryComparer = new QueryComparer(args[0],args[1]);
            }
            else
            {
                queryComparer = new QueryComparer();
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var queries = queryComparer.ReadQueryList();
            var vectors = queryComparer.CreateQueryLists(queries);
            queryComparer.CompareQueries(vectors);
            
            stopwatch.Stop();
            Console.WriteLine("\nAll computations took: " + stopwatch.Elapsed.ToString("c"));
            Console.ReadKey();
        }
    }
}
