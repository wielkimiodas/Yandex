using System;

namespace Yandex.Transfer
{
    internal class Program
    {
        private const string connstr = "Server=localhost;Port=5432;User Id=postgres;Password=qwerty;Database=postgres;";

        private static void Main(string[] args)
        {
            args = new String[] { @"D:\Downloads\EDWD\train_tr" };
            DateTime begin = DateTime.Now;

            //using (Transfer t = new Transfer(connstr, "train", @"C:\tmp1\"))
            //{
            //    t.transfer(args[0]);
            //}

            using (var log = new LogTableInitializer(connstr,"train")            )
            {
                log.transfer(@"C:\Users\Wojciech\Desktop\test_out.txt");
            }

            Console.WriteLine("Total time: " + (DateTime.Now - begin));
            Console.ReadLine();
        }
    }
}