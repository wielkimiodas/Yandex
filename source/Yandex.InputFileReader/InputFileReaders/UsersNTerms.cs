using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;
using System.IO;
using System.Diagnostics;

namespace Yandex.InputFileReader
{
    public class UsersNTerms : InputFileReader
    {
        List<List<int>> usersTerms = null;
        List<int> currentList;
        string outputFile;

        public UsersNTerms(String outputFile)
        {
            this.outputFile = outputFile;
        }

        public override void onBeginRead()
        {
            usersTerms = new List<List<int>>();
            currentList = null;
        }

        public List<int> getList(int userId)
        {
            while (usersTerms.Count < userId)
                usersTerms.Add(null);

            if (usersTerms.Count == userId)
                usersTerms.Add(new List<int>());

            List<int> list = usersTerms[userId];
            if (list == null)
            {
                list = new List<int>();
                usersTerms[userId] = list;
            }

            return list;
        }

        public override void onMetadata(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // DAY
            reader.ReadInt32();

            // USER
            int userId = reader.ReadInt32();
            currentList = getList(userId);
        }

        public override void onQueryAction(BufferedBinaryReader reader)
        {
            // TYPE
            reader.ReadByte();

            // SESSION_ID
            reader.ReadInt32();

            // TIME
            reader.ReadInt32();
            // SERPID
            reader.ReadInt32();
            // QUERYID
            reader.ReadInt32();

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // TERM ID
                int termId = reader.ReadInt32();
                currentList.Add(termId);
            }   

            for (int i = reader.ReadInt32(); i > 0; i--)
            {
                // URL_ID
                reader.ReadInt32();

                // DOMAIN_ID
                reader.ReadInt32();
            }
        }

        public override void onEndRead()
        {
            List<Tuple<int, List<Tuple<int, int>>>> bigList = new List<Tuple<int, List<Tuple<int, int>>>>();

            for (int i = 0; i < usersTerms.Count; i++)
            {
                if (i % 10000 == 0)
                {
                    Console.Write("                  \rRedoing: {0} %\r", (100.0f * i / usersTerms.Count).ToString("0.000"));
                    if (i % 500000 == 0)
                        GC.Collect();
                }

                List<int> list = usersTerms[i];
                usersTerms[i] = null;
                if (list == null)
                    continue;

                list.Sort();
                List<Tuple<int, int>> occurrences = new List<Tuple<int, int>>();
                int index = 0;
                while (index < list.Count)
                {
                    int start = index;
                    while (index < list.Count - 1)
                    {
                        if (list[index] != list[index + 1])
                            break;
                        index++;
                    }

                    occurrences.Add(new Tuple<int, int>(list[index], index - start + 1));
                    index++;
                }

                /*
                writer.WriteLine("User {0}:", i);
                foreach (var occurrence in occurrences)
                    writer.WriteLine("{0}\t{1}", occurrence.Item1, occurrence.Item2);
                writer.WriteLine();*/
                bigList.Add(new Tuple<int, List<Tuple<int, int>>>(i, occurrences));
            }

            Console.Write("                 \r");
            usersTerms = null;
            GC.Collect();

            bigList.Sort((o1, o2) =>
            {
                int c1 = o1.Item2.Count;
                int c2 = o2.Item2.Count;
                if (c1 < c2)
                    return 1;
                if (c1 > c2)
                    return -1;
                return o1.Item1 - o2.Item1;
            }
            );

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                foreach (var element in bigList)
                {
                    writer.WriteLine("User {0}:", element.Item1);
                    foreach (var occurrence in element.Item2)
                        writer.WriteLine("{0}\t{1}", occurrence.Item1, occurrence.Item2);
                    writer.WriteLine();
                }
            }
        }
    }
}
