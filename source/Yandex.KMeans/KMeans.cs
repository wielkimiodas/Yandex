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

            /*var termsCounts = new List<int>();
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

            var list = new BinarySearchMultiSet<Tuple<int, int>>(new CmpTuple());
            for (int i = 0; i < termsCounts.Count; i++)
            {
                if (list.list.Count > 3 * finalAvg)
                {
                    list.list.RemoveRange(finalAvg, list.list.Count - finalAvg);
                }

                list.Add(new Tuple<int, int>(i, termsCounts[i]));
            }

            list.list.RemoveRange(finalAvg, list.list.Count - finalAvg);

            var centroidTerms = new BinarySearchSet<int>(Comparer<int>.Default);
            foreach(var val in list)
                centroidTerms.Add(val.Item1);*/

            return new User() { userId = -1, terms = allTerms };
        }
    }

    public class KMeans
    {
        const int N_HASHES = 30;
        public const int MAX_TERM_ID = 4853846;

        public static void DoKMeans(String filename, String output)
        {
            var watch = Stopwatch.StartNew();
            var matrix = MatrixReader.GetMatrix(PathResolver.UserMatrix);
            watch.Stop();
            Console.WriteLine("Matrix loading:\t" + watch.Elapsed);

            watch = Stopwatch.StartNew();
            watch.Stop();
            Console.WriteLine("Calculate indexes:\t" + watch.Elapsed);

            var r = new Random();

            watch = Stopwatch.StartNew();
            List<User> allUsers = User.GetUsers(matrix);
            allUsers.Sort((u1, u2) => { return u2.terms.Count - u1.terms.Count; });
            watch.Stop();
            Console.WriteLine("Getting users:\t" + watch.Elapsed);

            matrix = null;
            GC.Collect();
            watch = Stopwatch.StartNew();
            int N_BOXES = allUsers.Count / 10000;
            List<User>[] boxes = new List<User>[N_BOXES + 1];
            boxes[N_BOXES] = new List<User>();
            {
                ParallelWorker tmpWorker = new ParallelWorker(8);

                User[] tmpCentroids = new User[N_BOXES];
                for (int it = 0; it < allUsers.Count; it++)
                {
                    if (it < N_BOXES)
                    {
                        boxes[it] = new List<User>();
                        boxes[it].Add(allUsers[it]);

                        tmpCentroids[it] = allUsers[it];
                    }
                    else
                    {
                        int i = it;
                        tmpWorker.Queue(delegate
                        {
                            float bestSim = -float.MaxValue;
                            int bestBox = -1;
                            for (int k = 0; k < tmpCentroids.Length; k++)
                            {
                                float sim = allUsers[i].GetSim(tmpCentroids[k]);
                                if (bestSim < sim)
                                {
                                    bestSim = sim;
                                    bestBox = k;
                                }
                            }

                            lock (boxes[bestBox])
                            {
                                boxes[bestBox].Add(allUsers[i]);
                            }
                        });
                    }
                    if (it % 100 == 0)
                        Console.Write("Users: {0:0.00}%\r", 100.0f * it / allUsers.Count);
                }

                tmpWorker.Wait();
            }
            watch.Stop();
            Console.WriteLine("Putting in boxes:\t" + watch.Elapsed);

            #region WYLICZANIE STATYSTYK
            {
                float avgCount = 0;
                float dev = 0;

                for (int b = 0; b < boxes.Length; b++)
                    avgCount += boxes[b].Count;
                avgCount /= boxes.Length;

                for (int b = 0; b < boxes.Length; b++)
                    dev += (float)Math.Pow(boxes[b].Count - avgCount, 2);
                dev = (float)Math.Sqrt(dev / boxes.Length);
                Console.WriteLine("Stats: " + boxes.Length + "\t" + avgCount + "\t" + dev);
                for (int b = 0; b < boxes.Length; b++)
                    Console.Write(boxes[b].Count + "\t");
                Console.WriteLine();
            }
            #endregion WYLICZANIE STATYSTYK

            ParallelWorker worker = new ParallelWorker(8);

            const int ITERATIONS = 20;
            for (int i = 1; i <= ITERATIONS; i++)
            {
                #region ZAPISYWANIE GRUPY DO PLIKU
                using (var writer = new StreamWriter(output + i + ".txt"))
                {
                    for (int j = 0; j < boxes.Length - 1; j++)
                    {
                        foreach (User user in boxes[j])
                            writer.WriteLine(user.userId);

                        writer.WriteLine();
                    }
                }
                #endregion ZAPISYWANIE GRUPY DO PLIKU

                watch = Stopwatch.StartNew();
                var newBoxes = new List<User>[boxes.Length];
                for (int j = 0; j < newBoxes.Length; j++)
                    newBoxes[j] = new List<User>();

                var centroids = new User[boxes.Length - 1];
                for (int j = 0; j < boxes.Length - 1; j++)
                {
                    int value = j;
                    Console.Write("Centroid: {0:0.00}%\r", 100.0f * j / boxes.Length);
                    worker.Queue(delegate
                    {
                        centroids[value] = User.GetCentroid(boxes[value]);
                    });
                }

                worker.Wait();

                Console.Write(new String(' ', 40) + "\r");

                for (int j = 0; j < boxes.Length; j++)
                {
                    Console.Write("Box: {0:0.00}%\r", 100.0f * j / boxes.Length);
                    foreach (User userIt in boxes[j])
                    {
                        User user = userIt;
                        worker.Queue(delegate
                        {
                            float bestSim = -float.MaxValue;
                            int bestBox = -1;
                            for (int k = 0; k < boxes.Length - 1; k++)
                            {
                                float sim = user.GetSim(centroids[k]);
                                if (bestSim < sim)
                                {
                                    bestSim = sim;
                                    bestBox = k;
                                }
                            }

                            const float MIN_SIM = 0.2f;

                            if (bestSim < MIN_SIM)
                                bestBox = boxes.Length - 1;

                            lock (newBoxes[bestBox])
                            {
                                newBoxes[bestBox].Add(user);
                            }
                        });
                    }

                    worker.Wait();
                }

                {
                    int nBoxes = newBoxes.Count((l) => { return l.Count != 0; });
                    if (newBoxes[newBoxes.Length - 1].Count == 0)
                        nBoxes++;
                    boxes = new List<User>[nBoxes];
                    int index = 0;
                    for (int j = 0; j < newBoxes.Length; j++)
                    {
                        if (newBoxes[j].Count == 0 && j != newBoxes.Length - 1)
                            continue;

                        boxes[index++] = newBoxes[j];
                    }
                }

                #region WYLICZANIE STATYSTYK
                {
                    float avgCount = 0;
                    float dev = 0;

                    for (int b = 0; b < boxes.Length; b++)
                        avgCount += boxes[b].Count;
                    avgCount /= boxes.Length;

                    for (int b = 0; b < boxes.Length; b++)
                        dev += (float)Math.Pow(boxes[b].Count - avgCount, 2);
                    dev = (float)Math.Sqrt(dev / boxes.Length);
                    Console.WriteLine("Stats: " + boxes.Length + "\t" + avgCount + "\t" + dev);
                    /*for (int b = 0; b < boxes.Length; b++)
                        Console.Write(boxes[b].Count + "\t");
                    Console.WriteLine();*/
                }
                #endregion WYLICZANIE STATYSTYK

                watch.Stop();
                Console.WriteLine("Iteration " + i + ":\t" + watch.Elapsed);
            }
        }
    }
}
