using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.IO;
using System.Diagnostics;

namespace Yandex.Grouper
{
    public class Grouper : IDisposable
    {
        /// <summary>
        /// Nazwa schematu w bazie danych.
        /// </summary>
        private readonly String schemaName;

        private const int MAX_CMD_TIMEOUT = 2*3600;

        private NpgsqlConnection connection = null;
        private NpgsqlTransaction transaction = null;

        StreamWriter writer = null;

        public Grouper(String connstring, String schemaName, String output, bool transactional = false)
        {
            this.schemaName = schemaName;
            connection = new NpgsqlConnection(connstring);
            connection.Open();

            if (transactional)
                transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            writer = new StreamWriter(output);
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }

            connection.Close();
            connection.Dispose();
            connection = null;

            writer.Dispose();
            writer = null;
        }

        private void doGroupBy(string columnsList)
        {
            string cmdText = String.Format("SELECT COUNT(*), {1} FROM {0}.log GROUP BY {1};", schemaName, columnsList);
            writer.WriteLine(cmdText);

            using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, connection))
            {
                cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                
                var watch = Stopwatch.StartNew();
                using (var result = cmd.ExecuteReader())
                {
                    watch.Stop();
                    writer.WriteLine(watch.ElapsedMilliseconds);

                    while (result.Read())
                    {
                        for (int i = 0; i < result.FieldCount; i++)
                            writer.Write(result[i] + "\t");
                        writer.WriteLine();
                    }
                    writer.WriteLine();
                }
            }

            //Console.WriteLine(columnsList);
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

        private void doAllGroupBy(int depth)
        {
            doAllGroupBy(0, new int[depth], String.Empty);
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

        public void group()
        {
            doAllGroupBy(1);
            //doAllGroupBy(2);
        }
    }
}
