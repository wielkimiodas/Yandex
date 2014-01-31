using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;
using System.Diagnostics;

namespace Yandex.KMeans
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            KMeans.DoKMeans(PathResolver.UserMatrix, @"D:\Downloads\EDWD\usersFinal");
            watch.Stop();

            Console.WriteLine("Groping took {0}", watch.Elapsed);
            Console.ReadLine();
        }
    }
}
