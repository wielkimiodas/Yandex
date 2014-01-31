using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace Yandex.LogProcessor
{
    class User
    {
        public int Id { get; set; }
        public List<Tuple<int,int>> Terms { get; set; }
    }
    public class UserMatrixCreator : IDisposable
    {
        private List<User> users;

        public void ReadUsersAndTerms(string filePath)
        {
            var reader = new StreamReader(filePath);
            users = new List<User>();

            while (reader.Peek()>-1)
            {
                var userLine = reader.ReadLine();
                var spaceIndex = userLine.IndexOf(' ');
                var userId = userLine.Substring(spaceIndex+1,userLine.Length-spaceIndex-2);

                var user = new User {Id = Convert.ToInt32(userId), Terms = new List<Tuple<int, int>>()};
                string term;
                while (!string.IsNullOrEmpty(term=reader.ReadLine()))
                {
                    var arr = term.Split('\t');
                    var tuple = new Tuple<int, int>(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1]));
                    user.Terms.Add(tuple);
                }
                users.Add(user);
            }
        }

        private static int CompareTwoUsers(User user1, User user2)
        {
            int i = 0, j = 0,similarity = 0;
            var list1 = user1.Terms;
            var list2 = user2.Terms;
            while (i < list1.Count && j < list2.Count)
            {
                if (list1[i].Item1 != list2[j].Item1)
                {
                    if (list1[i].Item1 < list2[j].Item1)
                        i++;
                    else
                        j++;
                }
                else
                {
                    similarity += list1[i].Item2*list2[j].Item2;
                    i++;
                    j++;
                }
            }
            return similarity;
        }

        public void CompareUsers()
        {
            double simSum = 0;
            int simCount = 0;
            //List<Tuple<UserId, List<Tuple<UserId, Similarity>>>>
            //matrix = new List<Tuple<int, List<Tuple<int, int>>>>();
            var count = users.Count;
            var path = PathResolver.UserMatrixOutput;
            var path2 = PathResolver.UserMatrixOutputProcessed;

            using (var writer = new BinaryWriter(new FileStream(path, FileMode.CreateNew)))
            {
                for (int i = 0; i < count; i++)
                {
                    var list = new List<Tuple<int, int>>();
                    for (int j = i + 1; j < count; j++)
                    {
                        var res = CompareTwoUsers(users[i], users[j]);
                        if (res == 0) continue;
                        list.Add(new Tuple<int, int>(j, res));
                        simSum += res;
                        simCount++;
                    }

                    //write UserId
                    writer.Write(list[i].Item1);
                    writer.Write(list.Count);
                    foreach (var element in list)
                    {
                        writer.Write(element.Item1);
                        writer.Write(element.Item2);
                    }
                    //matrix.Add(new Tuple<int, List<Tuple<int, int>>>(i,list));
                }
            }

            double minVal = 1.5 /* ?? */* simSum / simCount;

            using (var writer = new BinaryWriter(new FileStream(path2, FileMode.CreateNew)))
            using (var reader = new BufferedBinaryReader(path))
            {
                while (reader.PeekChar() > -1)
                {
                    int currentUser = reader.ReadInt32();
                    var usersList = new List<int>();
                    for(int i = reader.ReadInt32(); i > 0; i--)
                    {
                        int userId = reader.ReadInt32();
                        int sim = reader.ReadInt32();
                        if (sim > minVal)
                            usersList.Add(userId);
                    }

                    if (usersList.Count > 0)
                    {
                        writer.Write(currentUser);
                        writer.Write(usersList.Count);
                        foreach (var userId in usersList)
                            writer.Write(userId);
                    }
                }
            }
        }

        public void Dispose()
        {
            users = null;
        }
    }
}
