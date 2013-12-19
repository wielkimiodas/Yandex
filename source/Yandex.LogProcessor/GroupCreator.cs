using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace Yandex.LogProcessor
{
    public class GroupCreator
    {
        private List<Tuple<int, List<int>>> _list;
        public void ReadData()
        {
            var path = PathResolver.GetPath("UserMatrixOutput_processed");
            _list = new List<Tuple<int, List<int>>>();
            using (var reader = new BufferedBinaryReader(path))
            {
                while (reader.PeekChar() > -1)
                {
                    int currentUser = reader.ReadInt32();
                    var usersList = new List<int>();
                    for (int i = reader.ReadInt32(); i > 0; i--)
                    {
                        int userId = reader.ReadInt32();
                        usersList.Add(userId);
                    }
                    _list.Add(new Tuple<int, List<int>>(currentUser,usersList));
                }
            }
        }

        public List<StaticSortedList<int>> GetUsersGroups()
        {
            var result = new List<StaticSortedList<int>>();

            int processed = 0;

            foreach (var element in _list)
            {
                if (++processed % 1000 == 0)
                {
                    Console.Write("                      \r");
                    Console.Write("Processed: {0} %\r", 100.0f * processed / _list.Count);
                }

                StaticSortedList<int> destination = null;
                foreach (var group in result)
                {
                    foreach (var value in element.Item2)
                    {
                        if (group.Contains(value))
                        {
                            destination = group;
                            break;
                        }
                    }
                }
                if (destination == null)
                {
                    destination = new StaticSortedList<int>((o1, o2) => o1 - o2);
                    result.Add(destination);
                }

                var toAdd = new List<int> {element.Item1};
                toAdd.AddRange(element.Item2.Where(value => !destination.Contains(value)));
                foreach (var value in toAdd)
                    destination.Add(value);
            }
            return result;
        }
    }
}
