using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yandex.InputFileReader;
using Yandex.Utils;

namespace Yandex.LogPortioner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Log portioner execution");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var portioner = new Portioner();
            var opener = new InputFileOpener(PathResolver.TrainProcessedFile, portioner);
            opener.Read();
            stopwatch.Stop();
            Console.WriteLine("Elapsed: " + stopwatch.Elapsed.Seconds + "s.");
            Console.ReadKey();
        }
    }
}
