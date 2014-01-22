using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Yandex.Utils;

namespace Yandex.KMeans
{
    public class User
    {
        public int userId;
        public BinarySearchSet<int> terms;

        public static List<User> getUsers(List<Tuple<int, BinarySearchSet<int>>> matrix, Tuple<int, int>[] allParams)
        {
            List<User> users = new List<User>();
            
            for(int i = 0; i < matrix.Count; i++)
            {
                var row = matrix[i];
                User user = new User() { userId = row.Item1, terms = row.Item2 };
                users.Add(user);
            }

            return users;
        }

        public float getSim(User otherUser)
        {
            int both = 0;
            int single = 0;
            int index1 = 0, index2 = 0;
            int count1 = terms.Count;
            int count2 = otherUser.terms.Count;
            while (count1 > index1 && count2 > index2)
            {
                int val1 = terms.ElementAt(index1);
                int val2 = otherUser.terms.ElementAt(index2);
                if (val1 == val2)
                {
                    both++;
                    index1++;
                    index2++;
                }
                else
                {
                    if (val1 < val2)
                    {
                        index1++;
                    }
                    else
                    {
                        index2++;
                    }

                    single++;
                }
            }

            both += count1 - index1;
            both += count2 - index2;

            both += single;

            return (float)both / single;
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

        public static User getCentroid(List<User> users, Tuple<int, int>[] allParams)
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

            return new User() { userId = -1, terms = centroidTerms };
        }
    }

    public class KMeans
    {
        const int N_HASHES = 30;
        public const int MAX_TERM_ID = 4853846;

        public static void doKMeans(String filename, String output)
        {
            var watch = Stopwatch.StartNew();
            var matrix = MatrixReader.getMatrix(PathResolver.UserMatrix);
            watch.Stop();
            Console.WriteLine("Matrix loading:\t" + watch.Elapsed);

            watch = Stopwatch.StartNew();
            var allIndexes = Minhash.getAllParams(N_HASHES, MAX_TERM_ID);
            watch.Stop();
            Console.WriteLine("Calculate indexes:\t" + watch.Elapsed);

            Random r = new Random();

            watch = Stopwatch.StartNew();
            List<User> allUsers = User.getUsers(matrix, allIndexes);
            watch.Stop();
            Console.WriteLine("Getting users:\t" + watch.Elapsed);

            matrix = null;
            GC.Collect();
            watch = Stopwatch.StartNew();
            int N_BOXES = allUsers.Count / 10000;
            List<User>[] boxes = new List<User>[N_BOXES];
            for (int i = 0; i < boxes.Length; i++)
                boxes[i] = new List<User>();
            for (int i = 0; i < allUsers.Count; i++)
            {
                int box = 0;
                int it = Math.Min(20, allUsers[i].terms.Count);
                for (int j = 0; j < it; j++)
                    box += allUsers[i].terms.ElementAt(j);
                box = box % N_BOXES;
                boxes[box].Add(allUsers[i]);
            }
            watch.Stop();
            Console.WriteLine("Putting in boxes:\t" + watch.Elapsed);
            
            const int ITERATIONS = 20;
            for (int i = 0; i < ITERATIONS; i++)
            {
                watch = Stopwatch.StartNew();
                List<User>[] newBoxes = new List<User>[N_BOXES];
                for (int j = 0; j < newBoxes.Length; j++)
                    newBoxes[j] = new List<User>();

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

                watch.Stop();
                Console.WriteLine("Iteration " + i + ":\t" + watch.Elapsed);
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
