using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Yandex.GroupbyAnalyzer
{
    internal class Groupby
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
            Columns = columnsString.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries);
        }

        private void ProcessResultList(IEnumerable<string> results)
        {
            var dimensions = Convert.ToInt32(Math.Pow(2, Columns.Length));
            CommonQueries = new long[dimensions];

            foreach (var result in results)
            {
                var array = result.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
                int modifier = 0;
                for (int i = 1; i < array.Length; i++)
                {
                    if (array[i].Equals("True"))
                        modifier += (int) Math.Pow(2, array.Length - i - 1);
                }
                CommonQueries[modifier] = Convert.ToInt64(array[0]);
            }
        }
    }

    public class Analyzer
    {
        private const string InPath = @"D:\Downloads\group_result_train.txt";
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

        public void Show()
        {
            List<Groupby> singleGroups = new List<Groupby>();
            List<Groupby> doubleGroups = new List<Groupby>();

            foreach (var group in _groupbies)
            {
                if (group.Columns.Length == 1)
                    singleGroups.Add(group);

                if (group.Columns.Length == 2)
                    doubleGroups.Add(group);
            }

            foreach (var group in new List<Groupby>[] {singleGroups, doubleGroups})
            {
                Stats time = new Stats();
                Stats values = new Stats();

                foreach (var g in group)
                {
                    time.addValue(g.ExecutionTime);
                    foreach (var val in g.CommonQueries)
                        values.addValue((int) val);
                }

                time.calculate("Time");
                values.calculate("Values");
                Console.WriteLine();
            }
        }
    }

    public class Stats
    {
        private List<int> values = new List<int>();

        public void addValue(int value)
        {
            values.Add(value);
        }

        public void calculate(string name)
        {
            float avg = 0;
            float avgNonZero = 0;
            int nNonZero = 0;
            float dev = 0;
            float devNonZero = 0;
            int min = Int32.MaxValue;
            int max = Int32.MinValue;
            int nMin = 0;
            int nMax = 0;

            foreach (int i in values)
            {
                avg += i;
                if (i != 0)
                {
                    nNonZero++;
                    if (min > i)
                        min = i;
                    if (max < i)
                        max = i;
                }
            }

            avgNonZero = avg;
            avg /= values.Count;
            avgNonZero /= nNonZero;

            foreach (int i in values)
            {
                dev += (float) Math.Pow(i - avg, 2);
                if (i != 0)
                    devNonZero += (float) Math.Pow(i - avgNonZero, 2);
            }

            foreach (int i in values)
            {
                dev += (float) Math.Pow(i - avg, 2);
                if (i != 0)
                    devNonZero += (float) Math.Pow(i - avgNonZero, 2);

                if (i == min)
                    nMin++;
                if (i == max)
                    nMax++;
            }

            dev = (float) Math.Sqrt(dev/values.Count);
            devNonZero = (float) Math.Sqrt(devNonZero/nNonZero);

            Console.WriteLine(name + ":");
            Console.WriteLine("{0,-12}{1} ({2})", avg, dev, values.Count);
            Console.WriteLine("{0,-12}{1} ({2})", avgNonZero, devNonZero, nNonZero);
            Console.WriteLine("{0,-12}{1} ({2})", String.Format("{0} ({1})", min, nMin), max, nMax);
        }
    }
}