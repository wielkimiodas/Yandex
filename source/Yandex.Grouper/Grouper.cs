using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yandex.Grouper
{
    public class Grouper
    {
        /* doGroupBy(string columnsList)
         * otrzymujesz liste oddzielonych po przecinku nazw kolumn, po ktorych ma sie odbyc grupowanie
         * a calosc wywolujesz doAllGroupBy(ILOSC_GRUPOWANYCH_KOLUMN)*/

        private void doGroupBy(string columnsList)
        {
            Console.WriteLine(columnsList);
        }

        private void doAllGroupBy(int depth, int[] columns, string columnsList)
        {
            if (depth == columns.Length)
            {
                doGroupBy(columnsList);
            }
            else
            {
                string prefix = columnsList;
                if (!String.IsNullOrEmpty(prefix))
                    prefix += ", ";

                int min = 1;
                if (depth > 0)
                    min = columns[depth - 1] + 1;
                for (int i = min; i < 202 - columns.Length + depth; i++)
                {
                    columns[depth] = i;

                    doAllGroupBy(depth + 1, columns, prefix + getColumnName(i));
                }
            }
        }

        public bool doAllGroupBy(int depth)
        {
            doAllGroupBy(0, new int[depth], String.Empty);

            return true;
        }

        private string getColumnName(int columnNumber)
        {
            if (columnNumber == 0)
                return "query_id";

            if (columnNumber > 0 && columnNumber <= 100)
                return "url" + columnNumber;

            if (columnNumber > 100 && columnNumber <= 200)
                return "term" + (columnNumber - 100);

            throw new Exception("Invalid column number");
        }
    }
}
