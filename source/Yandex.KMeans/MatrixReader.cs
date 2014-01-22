using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yandex.Utils;

namespace Yandex.KMeans
{
    public class MatrixReader
    {
        public static List<Tuple<int, BinarySearchSet<int>>> GetMatrix(String filename)
        {
            var watch = Stopwatch.StartNew();

            var matrix = new List<Tuple<int, BinarySearchSet<int>>>();

            using (var reader = new BufferedBinaryReader(filename))
            {
                int lineCounter = 0;
                float length = reader.reader.BaseStream.Length / 100.0f;

                while (reader.PeekChar() > -1)
                {
                    if (++lineCounter % 100000 == 0)
                        Console.Write("                 \rRead: {0} %\r",
                            (reader.reader.BaseStream.Position / length).ToString("0.000"));

                    int userId = reader.ReadInt32();
                    int nTerms = reader.ReadInt32();
                    var list = new List<int>();
                    for (int i = 0; i < nTerms; i++)
                        list.Add(reader.ReadInt32());
                    matrix.Add(new Tuple<int, BinarySearchSet<int>>(userId, new BinarySearchSet<int>(list, Comparer<int>.Default)));
                }
            }

            watch.Stop();
            Console.WriteLine("Matrix loaded after: {0}", watch.Elapsed);

            return matrix;
        }
    }
}
