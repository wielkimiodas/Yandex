using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
    }
}
