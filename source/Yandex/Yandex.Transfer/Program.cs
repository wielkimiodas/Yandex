using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Transfer
{
    class Program
    {
        const string connstr = "Server=localhost;Port=5432;User Id=postgres;Password=qwerty;Database=postgres;";

        static void Main(string[] args)
        {
            DateTime begin = DateTime.Now;

            using (Transfer t = new Transfer(connstr, "NAZWA_SCHEMATU", @"ŚCIEŻKA DO WORK DIRA"))
            {
                t.transfer(args[0]);
            }

            Console.WriteLine("Total time: " + (DateTime.Now - begin));
        }
    }
}
