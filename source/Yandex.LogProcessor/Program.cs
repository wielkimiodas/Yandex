using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yandex.LogProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            QueryComparer lw;
            if (args.Count() == 1 && File.Exists(args[0]))
            {
                lw = new QueryComparer(args[0]);
            }
            else
            {
                lw = new QueryComparer();
            }

            var queries = lw.ReadQueryList();
            var vectors = lw.CreateQueryLists(queries);
            lw.CompareQueries2(vectors);
            //var res = vectors.Where(x => x.Id == 490145);
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
