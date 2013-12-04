using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Grouper
{
    class Program
    {
        static void Main(string[] args)
        {
            var depth = 1;
            var g = new Grouper();
            
            if (args.Count() == 1)
            {
                g.doAllGroupBy(Convert.ToInt32(args[0]));
            }
            else
            {
                g.doAllGroupBy(depth);
            }
        }
    }
}
