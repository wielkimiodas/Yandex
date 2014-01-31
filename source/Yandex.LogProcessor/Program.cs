using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Yandex.Utils;

namespace Yandex.LogProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            var processor = new FinalProcessor();
            processor.ProcessTestInput(PathResolver.TestProcessedFile, PathResolver.TestProcessedFile + "2" /*!!!!!!!! NAZWA NOWEGO PLIKU TESTOWEGO !!!!!!!!!"*/);
            watch.Stop();
            Console.WriteLine("Processing took {0}", watch.Elapsed);
            Console.ReadLine();

            /*var time = ExecuteUsersMatrix(args);

            Console.WriteLine("\nAll computations took: " + time.ToString("c"));
            Console.ReadKey();*/
        }

        private static TimeSpan ExecuteQueries(string[] args)
        {
            QueryComparer queryComparer;
            if (args.Count() == 2 && File.Exists(args[0]))
            {
                queryComparer = new QueryComparer(args[0], args[1]);
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

            return stopwatch.Elapsed;
        }

        private static TimeSpan ExecuteUsersMatrix(string[] args)
        {
            UserMatrixCreator userMatrixCreator = null;
            if (args.Count() == 2 && File.Exists(args[0]))
            {
                //queryComparer = new QueryComparer(args[0], args[1]);
            }
            else
            {
                userMatrixCreator = new UserMatrixCreator();
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            userMatrixCreator.ReadUsersAndTerms(PathResolver.UserMatrix);
            userMatrixCreator.CompareUsers();

            userMatrixCreator.Dispose();
            GC.Collect();

            var groupCreator = new GroupCreator();
            groupCreator.GetUsersGroups();


            stopwatch.Stop();

            return stopwatch.Elapsed;
        }
    }
}