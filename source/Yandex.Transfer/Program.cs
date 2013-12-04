using System;

namespace Yandex.Transfer
{
    internal class Program
    {
        private const string connstr = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres;";

        private static void Main(string[] args)
        {
            args = new String[] { @"D:\Downloads\EDWD\train_tr" };
            DateTime begin = DateTime.Now;

            //using (Transfer t = new Transfer(connstr, "train", @"C:\tmp1\"))
            //{
            //    t.transfer(args[0]);
            //}

            using (var log = new LogTableInitializer(connstr,"test"))
            {
                log.transfer(@"D:\Downloads\EDWD\test_output.txt");
            }

            Console.WriteLine("Total time: " + (DateTime.Now - begin));
            Console.ReadLine();
        }
    }
}