using System;
using System.Diagnostics;
using Yandex.InputFileReader.InputFileReaders;
using Yandex.Utils;

namespace Yandex.InputFileReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string data = "train";
            var readers = new InputFileReader[]
            {
                //new TopUrlsGetter(@"D:\Downloads\EDWD\"+data+"_top_urls_1.txt"),
                //new TopTermsGetter(@"D:\Downloads\EDWD\"+data+"_top_terms_1.txt"),
                //new TopDomainsGetter(@"D:\Downloads\EDWD\"+data+"_top_domains_1.txt"),
                //new QueriesExtractor(@"D:\Downloads\EDWD\"+data+"_queries"),
                //new UsersNTerms(@"D:\Downloads\EDWD\" + data + "_users2terms.txt"),
                //new DomainsToTerms(@"D:\Downloads\EDWD\" + data + "_domains2terms.txt"),
                //new DefaultRanking(PathResolver.OutputPath), 
                new LinkSorter(), 
                
            };

            //const string filename = @"D:\Downloads\EDWD\" + data + "_tr";
            var filename = PathResolver.TrainProcessedFile;

            foreach (var reader in readers)
            {
                var watch = Stopwatch.StartNew();
                using (var opener = new InputFileOpener(filename,
                    reader))
                {
                    opener.Read();
                }
                watch.Stop();
                Console.WriteLine("Time {0}", watch.Elapsed);
            }

            Console.ReadLine();

            /******* from old Portioner:
             * Console.WriteLine("Log portioner execution");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var portioner = new Portioner();
            var opener = new InputFileOpener(PathResolver.TrainProcessedFile, portioner);
            opener.Read();
            stopwatch.Stop();
            Console.WriteLine("Elapsed: " + stopwatch.Elapsed.TotalSeconds + "s.");
            Console.ReadKey();
             * **********/

        }
    }
}