using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.GroupbyAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var analyzer = new Analyzer();
            analyzer.ReadGroupbys();

            analyzer.Show();

            Console.ReadLine();
        }
    }
}
