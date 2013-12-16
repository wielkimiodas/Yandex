using System;

namespace Yandex.GroupbyAnalyzer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var analyzer = new Analyzer();
            analyzer.ReadGroupbys();

            analyzer.Show();

            Console.ReadLine();
        }
    }
}