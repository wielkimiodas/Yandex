using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace KMeans
{
    class Minhash
    {
        public static int[] getMinhashValues(BinarySearchSet<int> row, int[][] allIndexes)
        {
            int[] result = new int[allIndexes.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = getMinhashValue(row, allIndexes[i]);

            return result;
        }

        private static int getMinhashValue(BinarySearchSet<int> row, int[] indexes)
        {
            int length = indexes.Length;
            for (int i = 0; i < length; i++)
            {
                if (row.Contains(indexes[i]))
                    return i;
            }

            throw new Exception("Nie znaleziono");
        }

        public static int getFuncVal(int iteration, int a, int b, int n)
        {
            return (a * iteration + b) % n;
        }

        public static int[][] getAllIndexes(int nHashes, int length)
        {
            int[][] result = new int[nHashes][];
            var allParams = getParams(nHashes, length);

            for(int i = 0; i< nHashes; i++)
            {
                result[i] = new int[length];
                for(int j = 0; j < length; j++)
                    result[i][j] = getFuncVal(j, allParams[i].Item1, allParams[i].Item2, length);
            }

            return result;
        }

        private static Tuple<int, int>[] getParams(int nHashes, int length)
        {
            Tuple<int, int>[] hashes = new Tuple<int, int>[nHashes];

            for (int i = 0; i < nHashes; i++)
            {
                int A = 0; // to do
                int B = 0; // to do
                hashes[i] = new Tuple<int, int>(A, B);
            }

            return hashes;
        }
    }
}
