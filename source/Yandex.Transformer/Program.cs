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
            if (args.Length != 2)
                return;

            DateTime dt = DateTime.Now;

            new Program().transform(args[0], args[1]);

            Console.WriteLine((DateTime.Now - dt));
        }
    }
}