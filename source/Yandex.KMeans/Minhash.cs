using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace Yandex.KMeans
{
    class Minhash
    {
        public static int[] getMinhashValues(BinarySearchSet<int> row, Tuple<int, int>[] allParams)
        {
            int[] result = new int[allParams.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = getMinhashValue(row, allParams[i]);

            return result;
        }

        private static int getMinhashValue(BinarySearchSet<int> row, Tuple<int, int> par)
        {
            int length = KMeans.MAX_TERM_ID;
            for (int i = 0; i < length; i++)
            {
                int value = getFuncVal(i, par.Item1, par.Item2, KMeans.MAX_TERM_ID);
                if (row.Contains(value))
                    return i;
            }

            return -1;
        }

        public static int getFuncVal(int iteration, int a, int b, int n)
        {
            return (a * iteration + b) % n;
        }

        public static Tuple<int, int>[] getAllParams(int nHashes, int length)
        {
            Random r = new Random();
            Tuple<int, int>[] hashes = new Tuple<int, int>[nHashes];

            for (int i = 0; i < nHashes; i++)
            {
                int A = 2 * r.Next(length / 2) + 1;
                int B = 2 * r.Next(length / 2) + 1;
                hashes[i] = new Tuple<int, int>(A, B);
            }

            return hashes;
        }
    }
}
