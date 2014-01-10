using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace KMeans
{
    public class User
    {
        public int userId;
        public BinarySearchSet<int> terms;
        public int[] hashValues;

        public static List<User> getUsers(List<Tuple<int, BinarySearchSet<int>>> matrix, int[][] allIndexes)
        {
            List<User> users = new List<User>();
            
            foreach (var row in matrix)
            {
                User user = new User() { userId = row.Item1, terms = row.Item2, hashValues = Minhash.getMinhashValues(row.Item2, allIndexes) };
            }

            return users;
        }
    }

    public class KMeans
    {
        const int N_HASHES = 1000;
        const int MAX_TERM_ID = 4853846;

        public static void doKMeans(String filename, String output)
        {
            var matrix = MatrixReader.getMatrix(PathResolver.UserMatrix);
            var allIndexes = Minhash.getAllIndexes(N_HASHES, MAX_TERM_ID);
            
        }
    }
}
