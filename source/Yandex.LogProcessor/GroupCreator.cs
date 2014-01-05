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
        
        private void ReadData()
        {
            var path = PathResolver.UserMatrixOutputProcessed;
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

        public List<BinarySearchList<int>> GetUsersGroups()
        {
            ReadData();

            var result = new List<BinarySearchList<int>>();

            int processed = 0;

            foreach (var element in _list)
            {
                if (++processed % 1000 == 0)
                {
                    Console.Write("                      \r");
                    Console.Write("Processed: {0} %\r", 100.0f * processed / _list.Count);
                }

                BinarySearchList<int> destination = null;
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
                    destination = new BinarySearchList<int>(new DefaultIntComparer());
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
