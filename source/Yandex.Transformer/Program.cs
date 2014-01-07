using System;
using System.IO;
using Yandex.Utils.UserActions;

namespace Yandex.Transformer
{
    internal class Program
    {
        private void transform(String inputFilename, String outputFilename)
        {
            char[] fieldsSep = {'\t'};
            char[] commaSep = {','};

            int lineCounter = 0;

            using (var reader = new StreamReader(inputFilename))
            using (var writer = new BinaryWriter(new FileStream(outputFilename, FileMode.Create)))
            {
                while (reader.Peek() > -1)
                {
                    lineCounter++;
                    String line = reader.ReadLine();
                    if (lineCounter==4426369)
                    {
                        int a = 4;
                    }
                    UserAction action = UserAction.GetAction(line);
                    if (action == null)
                    {
                        Console.WriteLine("Incorrect line #" + lineCounter + ":\t" + line);
                        continue;
                    }

                    if (!action.WriteToFile(writer))
                    {
                        Console.WriteLine("IOException in line #" + lineCounter);
                        return;
                    }
                }
            }
        }

        private static void Main(string[] args)
        {
            //if (args.Length != 2)
              //  return;

            DateTime dt = DateTime.Now;
            var s1 = @"H:\Projects\EDWD\Personalized Web Search Challenge\data\train\train";
            var s2 = @"H:\Projects\EDWD\Personalized Web Search Challenge\data\train\train processed";
            new Program().transform(s1, s2);

            Console.WriteLine((DateTime.Now - dt));
            Console.ReadKey();
        }
    }
}