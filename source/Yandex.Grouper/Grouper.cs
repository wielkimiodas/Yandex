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

        StreamWriter writer = null;

        int id;
        int min;
        int max;

        public Grouper(String connstring, String schemaName, StreamWriter writer, int id, int min, int max)
        {
            this.schemaName = schemaName;
            connection = new NpgsqlConnection(connstring);
            connection.Open();

            this.writer = writer;

            this.id = id;
            this.min = min;
            this.max = max;
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
            connection = null;
        }

        private void doGroupBy(string columnsList)
        {
            string cmdText = String.Format("SELECT COUNT(*), {1} FROM {0}.log GROUP BY {1};", schemaName, columnsList);
            
            using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, connection))
            {
                cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                
                var watch = Stopwatch.StartNew();
                using (var result = cmd.ExecuteReader())
                {
                    watch.Stop();

                    lock (writer)
                    {
                        writer.WriteLine(cmdText);
                        writer.WriteLine(watch.ElapsedMilliseconds);

                        while (result.Read())
                        {
                            for (int i = 0; i < result.FieldCount; i++)
                                writer.Write(result[i] + "\t");
                            writer.WriteLine();
                        }
                        writer.WriteLine();
                        writer.Flush();
                    }
                }
            }
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
                int max = 1;
                if (depth > 0)
                {
                    min = columns[depth - 1] + 1;
                    max = 201 + 1 - columns.Length + depth;
                }
                else
                {
                    min = this.min;
                    max = this.max;
                }
                for (int i = min; i < max; i++)
                {
                    lock (Console.Out)
                    {
                        Console.SetCursorPosition(depth * 5, this.id);
                        Console.Write(i);
                    }
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
            doAllGroupBy(2);
        }
    }
}
