using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Utils;

namespace Yandex.LogProcessor
{
    public class GroupCreator
    {
        static List<StaticSortedList<int>> GetUsersGroups(List<Tuple<int, List<int>>> list)
        {
            var result = new List<StaticSortedList<int>>();

            int processed = 0;

            foreach (var element in list)
            {
                if (++processed % 1000 == 0)
                {
                    Console.Write("                      \r");
                    Console.Write("Processed: {0} %\r", 100.0f * processed / list.Count);
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
                    destination = new StaticSortedList<int>((o1, o2) =>
                    {
                        return o1 - o2;
                    });
                    result.Add(destination);
                }

                List<int> toAdd = new List<int>();
                toAdd.Add(element.Item1);
                foreach (var value in element.Item2)
                    if (!destination.Contains(value))
                        toAdd.Add(value);
                foreach (var value in toAdd)
                    destination.Add(value);
            }

            return result;
        }

        //public static void test()
        //{
        //    List<Tuple<int, List<int>>> list = new List<Tuple<int, List<int>>>();
        //    list.Add(new Tuple<int, List<int>>(1, new List<int>()));
        //    list.Add(new Tuple<int, List<int>>(2, new List<int>(new int[] { 1 })));
        //    list.Add(new Tuple<int, List<int>>(3, new List<int>(new int[] { })));
        //    list.Add(new Tuple<int, List<int>>(4, new List<int>(new int[] { 3 })));
        //    list.Add(new Tuple<int, List<int>>(5, new List<int>(new int[] { 2 })));
        //    list.Add(new Tuple<int, List<int>>(6, new List<int>(new int[] { 3 })));
        //    foreach (var lst in getUsersGroups(list))
        //    {
        //        Console.WriteLine("Nowa grupa:");
        //        foreach (var value in lst)
        //            Console.Write(value + " ");
        //        Console.WriteLine();
        //    }
        //}
    }
}
