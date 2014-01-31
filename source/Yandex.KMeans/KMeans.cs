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

        public static List<User> GetUsers(List<Tuple<int, BinarySearchSet<int>>> matrix)
        {
            var result = matrix.Select(row => new User() {userId = row.Item1, terms = row.Item2}).ToList();
            result.RemoveRange(result.Count, result.Count - result.Count);
            return result;
        }

        public float GetSim(User otherUser)
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

            single += both;

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

        public static User GetCentroid(List<User> users)
        {
            var allTerms = new BinarySearchSet<int>(Comparer<int>.Default);
            foreach (var user in users)
                foreach (var term in user.terms)
                    allTerms.Add(term);

            return new User() { userId = -1, terms = allTerms };
        }
    }

    public class KMeans
    {
        const int N_HASHES = 30;
        public const int MAX_TERM_ID = 4853846;

        private static List<List<User>> getSmallBoxes(List<User> allUsers)
        {
            allUsers.Sort((u1, u2) => { return u1.terms.Count - u2.terms.Count; });
            List<List<User>> boxes = new List<List<User>>();
            ParallelWorker tmpWorker = new ParallelWorker(8);

            const float MIN_SIM = 0.7f;

            for (int it = 0; it < allUsers.Count; it++)
            {
                int i = it;
                tmpWorker.Queue(delegate
                {
                    float bestSim = 0;
                    int bestBox = -1;
                    int nBoxes;
                    lock (boxes)
                    {
                        nBoxes = boxes.Count;
                    }
                    for (int k = 0; k < nBoxes; k++)
                    {
                        float sim = allUsers[i].GetSim(boxes[k][0]);
                        if (bestSim < sim)
                        {
                            bestSim = sim;
                            bestBox = k;
                        }
                    }

                    lock (boxes)
                    {
                        for (int k = nBoxes; k < boxes.Count; k++)
                        {
                            float sim = allUsers[i].GetSim(boxes[k][0]);
                            if (bestSim < sim)
                            {
                                bestSim = sim;
                                bestBox = k;
                            }
                        }

                        if (bestSim < MIN_SIM)
                        {
                            bestBox = boxes.Count;
                            boxes.Add(new List<User>());
                        }

                        boxes[bestBox].Add(allUsers[i]);
                    }
                });
                if (it % 1000 == 0)
                    Console.Write("Users: {0:0.00}%\t({1})\r", 100.0f * it / allUsers.Count, boxes.Count);
            }

            tmpWorker.Wait();

            return boxes;
        }

        private static List<List<User>> getBoxes(List<User> allUsers2)
        {
            List<List<User>> allBoxes = new List<List<User>>();

            const int N = 100;
            for (int b = 0; b < N; b++)
            {
                int min = b * allUsers2.Count / N;
                int max = (b + 1) * allUsers2.Count / N;
                var users = allUsers2.GetRange(min, max - min);
                var boxes2 = getSmallBoxes(users);

                const float MIN_SIM = 0.5f;

                ParallelWorker tmpWorker = new ParallelWorker(8);
                for (int it = 0; it < boxes2.Count; it++)
                {
                    int i = it;
                    tmpWorker.Queue(delegate
                    {
                        float bestSim = 0;
                        int bestBox = -1;
                        int nBoxes;
                        lock (allBoxes)
                        {
                            nBoxes = allBoxes.Count;
                        }
                        for (int k = 0; k < nBoxes; k++)
                        {
                            float sim = boxes2[i][0].GetSim(allBoxes[k][0]);
                            if (bestSim < sim)
                            {
                                bestSim = sim;
                                bestBox = k;
                            }
                        }

                        lock (allBoxes)
                        {
                            for (int k = nBoxes; k < allBoxes.Count; k++)
                            {
                                float sim = boxes2[i][0].GetSim(allBoxes[k][0]);
                                if (bestSim < sim)
                                {
                                    bestSim = sim;
                                    bestBox = k;
                                }
                            }

                            if (bestSim < MIN_SIM)
                            {
                                bestBox = allBoxes.Count;
                                allBoxes.Add(new List<User>());
                            }

                            allBoxes[bestBox].AddRange(boxes2[i]);
                        }
                    });
                }
                tmpWorker.Wait();

                Console.WriteLine("Done {0}%", 100.0f * b / N);
            }

            return allBoxes;
        }

        public static void DoKMeans(String filename, String output)
        {
            var watch = Stopwatch.StartNew();
            var matrix = MatrixReader.GetMatrix(PathResolver.UserMatrix);
            watch.Stop();
            Console.WriteLine("Matrix loading:\t" + watch.Elapsed);

            var r = new Random();

            watch = Stopwatch.StartNew();
            List<User> allUsers = User.GetUsers(matrix);
            //allUsers.Sort((u1, u2) => { return u1.terms.Count - u2.terms.Count; });
            watch.Stop();
            Console.WriteLine("Getting users:\t" + watch.Elapsed);

            matrix = null;
            GC.Collect();
            watch = Stopwatch.StartNew();
            List<List<User>> boxes = new List<List<User>>();

            boxes = getBoxes(allUsers);

            watch.Stop();
            Console.WriteLine("Putting in boxes:\t" + watch.Elapsed);

            #region WYLICZANIE STATYSTYK
            {
                float avgCount = 0;
                float dev = 0;

                for (int b = 0; b < boxes.Count; b++)
                    avgCount += boxes[b].Count;
                avgCount /= boxes.Count;

                for (int b = 0; b < boxes.Count; b++)
                    dev += (float)Math.Pow(boxes[b].Count - avgCount, 2);
                dev = (float)Math.Sqrt(dev / boxes.Count);
                Console.WriteLine("Stats of {0} boxes:", boxes.Count);
                Console.WriteLine("  AVG(Count):\t" + avgCount + "\tDev:\t" + dev);
            }
            #endregion WYLICZANIE STATYSTYK

            ParallelWorker worker = new ParallelWorker(8);

            #region ZAPISYWANIE GRUPY DO PLIKU
            using (var writer = new StreamWriter(output))
            {
                for (int j = 0; j < boxes.Count - 1; j++)
                {
                    foreach (User user in boxes[j])
                        writer.WriteLine(user.userId);

                    writer.WriteLine();
                }
            }
            #endregion ZAPISYWANIE GRUPY DO PLIKU
        }
    }
}
