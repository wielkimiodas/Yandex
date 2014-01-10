using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yandex.Coprimes
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            for(int i=0;i<10000;i++)
            {
                var o = CoPrime.GetCoprimeTuple();
                Console.WriteLine(o.Item1 + " " + o.Item2);
            }
            watch.Stop();
            Console.WriteLine("took: "+ watch.Elapsed.TotalSeconds);
            Console.ReadKey();

        }
    }
}