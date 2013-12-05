using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Yandex.GroupbyAnalyzer
{
    class Groupby
    {
        public int ExecutionTime { get; set; }
        public string[] Columns { get; set; }
        public long[] CommonQueries { get; set; }

        public Groupby(string query, int timespan, IEnumerable<string> results)
        {
            ReadColumns(query);
            ExecutionTime = timespan;
            ProcessResultList(results);
        }

        private void ReadColumns(string query)
        {
            var matches = Regex.Matches(query, @"SELECT COUNT\(\*\), (.*) FROM");
            var columnsString = matches[0].Groups[1].ToString();
            Columns = columnsString.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void ProcessResultList(IEnumerable<string> results)
        {
            var dimensions = Convert.ToInt32(Math.Pow(2, Columns.Length));
            CommonQueries = new long[dimensions];

            foreach (var result in results)
            {
                var array = result.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int modifier = 0;
                for (int i = 1; i < array.Length; i++)
                {
                    if (array[i].Equals("True"))
                        modifier += (int)Math.Pow(2, array.Length - i - 1);
                }
                CommonQueries[modifier] = Convert.ToInt64(array[0]);
            }
        }
    }

    public class Analyzer
    {
        private const string InPath = @"C:\Users\Wojciech\Desktop\group_result_train.txt";
        private readonly List<Groupby> _groupbies = new List<Groupby>();

        public void ReadGroupbys()
        {
            var reader = new StreamReader(InPath);
            while (reader.Peek() != -1)
            {
                string tmp = reader.ReadLine();
                var query = tmp;
                tmp = reader.ReadLine();
                var timeInMs = Convert.ToInt32(tmp);
                tmp = reader.ReadLine();

                var resultList = new List<string>();
                while (!string.IsNullOrEmpty(tmp))
                {
                    resultList.Add(tmp);
                    tmp = reader.ReadLine();
                }

                _groupbies.Add(new Groupby(query, timeInMs, resultList));
            }
        }
    }
}
