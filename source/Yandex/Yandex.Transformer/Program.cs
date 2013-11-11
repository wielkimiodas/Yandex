using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Transformer
{
    class Program
    {
        void transform(String inputFilename, String outputFilename)
        {
            char[] fieldsSep = new char[] { '\t' };
            char[] commaSep = new char[] { ',' };

            int lineCounter = 0;

            using (StreamReader reader = new StreamReader(inputFilename))
            //using (BinaryWriter writer = new BinaryWriter(new FileStream(outputFilename, FileMode.Create)))
            using (BufferedBinaryWriter writer = new BufferedBinaryWriter(outputFilename))
            {
                while (reader.Peek() > -1)
                {
                    lineCounter++;
                    String line = reader.ReadLine();

                    UserAction action = UserAction.getAction(line);
                    if (action == null)
                    {
                        Console.WriteLine("Incorrect line #" + lineCounter + ":\t" + line);
                        continue;
                    }

                    if (!action.writeToFile(writer))
                    {
                        Console.WriteLine("IOException in line #" + lineCounter);
                        return;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
                return;

            DateTime dt = DateTime.Now;

            new Program().transform(args[0], args[1]);

            Console.WriteLine((DateTime.Now - dt));
        }
    }
}
