using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

        public float getSim(User otherUser)
        {
            int nRelated = 0;
            for (int i = 0; i < hashValues.Length; i++)
            {
                if (hashValues[i] == otherUser.hashValues[i])
                    nRelated++;
            }

            return (float)nRelated / hashValues.Length;
        }

        class CmpTuple : Comparer<Tuple<int, int>>
        {
            public override int Compare(Tuple<int, int> x, Tuple<int, int> y)
            {
                if (x.Item2 == y.Item2)
                    return x.Item1.CompareTo(y.Item1);
                else
                    return x.Item2.CompareTo(y.Item2);
            }
        }

        public static User getCentroid(List<User> users, int[][] allIndexes)
        {
            List<int> termsCounts = new List<int>();
            float avg = 0;
            foreach (var user in users)
            {
                avg += user.terms.Count;
                foreach (int term in user.terms)
                {
                    while (termsCounts.Count <= term)
                        termsCounts.Add(0);

                    termsCounts[term]++;
                }
            }

            int finalAvg = (int)(avg / users.Count);

            BinarySearchMultiSet<Tuple<int, int>> list = new BinarySearchMultiSet<Tuple<int, int>>(new CmpTuple());
            for (int i = 0; i < termsCounts.Count; i++)
            {
                if (list.list.Count > 3 * finalAvg)
                {
                    list.list.RemoveRange(finalAvg, list.list.Count - finalAvg);
                }

                list.Add(new Tuple<int, int>(i, termsCounts[i]));
            }

            list.list.RemoveRange(finalAvg, list.list.Count - finalAvg);

            BinarySearchSet<int> centroidTerms = new BinarySearchSet<int>(Comparer<int>.Default);
            foreach(var val in list)
                centroidTerms.Add(val.Item1);

            return new User() { userId = -1, terms = centroidTerms, hashValues = Minhash.getMinhashValues(centroidTerms, allIndexes) };
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
            Random r = new Random();
            
            List<User> allUsers = User.getUsers(matrix, allIndexes);
            matrix = null;
            GC.Collect();
            int N_BOXES = allUsers.Count / 10000;
            List<User>[] boxes = new List<User>[N_BOXES];
            for (int i = 0; i < allUsers.Count; i++)
            {
                int box = 0;
                for (int j = 0; j < 20; j++)
                    box += allUsers[i].hashValues[j];
                box = box % N_BOXES;
                boxes[box].Add(allUsers[i]);
            }

            const int ITERATIONS = 20;
            for (int i = 0; i < ITERATIONS; i++)
            {
                List<User>[] newBoxes = new List<User>[N_BOXES];

                User[] centroids = new User[N_BOXES];
                for (int j = 0; j < N_BOXES; j++)
                {
                    centroids[j] = User.getCentroid(boxes[j], allIndexes);
                }

                for (int j = 0; j < N_BOXES; j++)
                {
                    foreach (User user in boxes[j])
                    {
                        float bestSim = -float.MaxValue;
                        int bestBox = -1;
                        for (int k = 0; k < N_BOXES; k++)
                        {
                            float sim = user.getSim(centroids[k]);
                            if (bestSim < sim)
                            {
                                bestSim = sim;
                                bestBox = k;
                            }
                        }

                        newBoxes[bestBox].Add(user);
                    }
                }

                boxes = newBoxes;
            }

            using (StreamWriter writer = new StreamWriter(output))
            {
                for (int i = 0; i < boxes.Length; i++)
                {
                    foreach (User user in boxes[i])
                        writer.WriteLine(user.userId);

                    writer.WriteLine();
                }
            }
        }
    }
}
