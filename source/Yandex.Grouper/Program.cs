using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Grouper
{
    class Program
    {
        private const string connstr = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres;";

        static void Main(string[] args)
        {
            string schema = "train";
            using (var g = new Grouper(connstr, schema, @"D:\Downloads\group_result_"+schema+".txt"))
            {
                g.group();
            }
        }
    }
}
