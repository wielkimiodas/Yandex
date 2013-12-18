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
    public class UserMatrixCreator
    {
        private List<User> users;
        private List<Tuple<int, List<Tuple<int, int>>>> matrix;

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
            //List<Tuple<UserId, List<Tuple<UserId, Similarity>>>>
            matrix = new List<Tuple<int, List<Tuple<int, int>>>>();
            var count = users.Count;
            var path = PathResolver.GetPath("UserMatrixOutput");
            var stream = new FileStream(path, FileMode.Create);
            var writer = new BinaryWriter(stream);

            for (int i = 0; i < count; i++)
            {
                var list = new List<Tuple<int, int>>();
                for (int j = i+1; j < count; j++)
                {
                    var res = CompareTwoUsers(users[i], users[j]);
                    list.Add(new Tuple<int, int>(j,res));
                }
                
                //writer.Write(); ???
                
                
                //matrix.Add(new Tuple<int, List<Tuple<int, int>>>(i,list));
            }
        }
    }
}
