using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yandex.UnitTest
{
    [TestClass]
    public class TransferTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string output = @"C:\Users\Wojciech\Desktop\logindexes.txt";
            var writer = new StreamWriter(output);

            //CREATE INDEX log_url1_index ON test.log (url1 ASC NULLS LAST);

            for (int i = 1; i < 101; i++)
            {
                writer.WriteLine("CREATE INDEX log_url_{0}_index ON test.log (url{0} ASC NULLS LAST);", i);
            }

            for (int i = 1; i < 101; i++)
            {
                writer.WriteLine("CREATE INDEX log_term_{0}_index ON test.log (term{0} ASC NULLS LAST);", i);
            }
        }
    }
}