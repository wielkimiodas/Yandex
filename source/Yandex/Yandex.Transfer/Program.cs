using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transfer
{
    class Program
    {
        const string connstr = "Server=localhost;Port=5432;User Id=postgres;Password=qwerty;Database=postgres;";

        static void Main(string[] args)
        {
            //args = new string[] { @"D:\Downloads\edwd\test_tr" };

            if (args.Length != 1)
                return;

            DateTime begin = DateTime.Now;

            using (Transfer t = new Transfer(connstr))
            {
                t.transfer(args[0]);
            }

            Console.WriteLine("Total time: " + (DateTime.Now - begin));
            //Console.ReadLine();
        }
    }
}
