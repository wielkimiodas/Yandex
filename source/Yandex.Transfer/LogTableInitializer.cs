using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Npgsql;

namespace Yandex.Transfer
{
    public class LogTableInitializer : IDisposable
    {
        private Dictionary<string, Tuple<string[], string[]>> tables =
            new Dictionary<string, Tuple<string[], string[]>>();
        private NpgsqlConnection connection = null;
        private NpgsqlTransaction transaction = null;

        private const string logTableName = "log";
        
        /// <summary>
        /// Nazwa użytkownika w bazie danych.
        /// </summary>
        private const string user = "postgres";

        /// <summary>
        /// Nazwa schematu w bazie danych.
        /// </summary>
        private readonly String schemaName;

        /// <summary>
        /// Katalog do przechowywania przetworzonych plików z danymi.
        /// </summary>
        //private readonly string workDir;

        /// <summary>
        /// Nawiązuje połączenie z bazą danych (i ew rozpoczyna transakcję).
        /// Tworzy definicję wszystkich tabel.
        /// </summary>
        /// <param name="connstring">Connstring używany do połączenia z bazą.</param>
        /// <param name="schemaName">Nazwa schematu, w którym mają zostać zapisane dane.</param>
        /// <param name="transactional">Określa, czy cały proces ma być wykonany transakcyjnie.</param>
        public LogTableInitializer(String connstring, String schemaName, bool transactional = false)
        {
            this.schemaName = schemaName;

            connection = new NpgsqlConnection(connstring);
            connection.Open();

            if (transactional)
                transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            tables.Add(logTableName, new Tuple<string[], string[]>(CreateColumnNamesArray(), CreateColumnTypesArray()));
        }

        /// <summary>
        /// Tworzy tabele.
        /// </summary>
        private bool createTables()
        {
            foreach (var element in tables)
            {
                String tableName = element.Key;
                var table = element.Value;

                if (table.Item1.Length != table.Item2.Length)
                    return false;

                String fields = String.Empty;
                for (int i = 0; i < table.Item1.Length; i++)
                {
                    String field = table.Item1[i] + " " + table.Item2[i];
                    if (fields.Equals(String.Empty))
                        fields = field;
                    else
                        fields += ", " + field;
                }

                String cmdStr = String.Format("CREATE TABLE {0}.{1} ( {2} ) WITH (OIDS=FALSE);", schemaName, tableName,
                    fields);
                NpgsqlCommand cmd = new NpgsqlCommand(cmdStr, connection);
                cmd.ExecuteNonQuery();

                cmdStr = String.Format("ALTER TABLE {0}.{1} OWNER TO {2};", schemaName, tableName, user);
                cmd = new NpgsqlCommand(cmdStr, connection);
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        private string inputFile;

        private delegate bool BoolFunction();
        /// <summary>
        /// Przeprowadza wszystkie czynności związane z umieszczeniem daynch w bazie danych.
        /// </summary>
        /// <param name="filename">Nazwa pliku z danymi.</param>
        public void transfer(String filename)
        {
            this.inputFile = filename;
            var functions = new BoolFunction[]
            {
                createTables,import, export
            };

            foreach (BoolFunction function in functions)
            {
                if (!function())
                {
                    rollback();
                    return;
                }

                if (transaction != null)
                {
                    transaction.Commit();
                    Console.WriteLine("Beginning new transaction");
                    transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                }
            }

            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
        }

        private Dictionary<int, List<int>> _queryLists;
        public bool import()
        {
            var q = ReadQueryList();
            _queryLists = CreateQueryLists(q);

            return true;
        }

        private List<int>[] _topUrlsAndTermsQueries;
        public HashSet<int> ReadQueryList()
        {
            _topUrlsAndTermsQueries = new List<int>[200];
            var textReader = new StreamReader(inputFile);
            var queries = new HashSet<int>();

            for (int i = 0; i < 200; i++)
            {
                _topUrlsAndTermsQueries[i] = new List<int>();
            }

            //******************
            //* urls segment   *
            //******************

            Console.Write("Processing urls... ");

            //eliminate count info
            var line = textReader.ReadLine();
            while (!line.Equals(""))
            {
                line = textReader.ReadLine();
            }

            //read url queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                var debug = textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (tmp != null && !tmp.Equals(""))
                {
                    var url = Convert.ToInt32(tmp);
                    _topUrlsAndTermsQueries[i].Add(url);
                    queries.Add(url);
                    tmp = textReader.ReadLine();
                }
            }

            //******************
            //* terms segment  *
            //******************

            //eliminate count info
            line = textReader.ReadLine();
            while (!line.Equals(""))
            {
                line = textReader.ReadLine();
            }

            //read term queries
            for (int i = 0; i < 100; i++)
            {
                //skip the line with description
                var debug = textReader.ReadLine();

                var tmp = textReader.ReadLine();
                while (tmp != null && !tmp.Equals(""))
                {
                    var term = Convert.ToInt32(tmp);
                    _topUrlsAndTermsQueries[i + 100].Add(term);
                    queries.Add(term);
                    tmp = textReader.ReadLine();
                }
            }

            textReader.Close();
            Console.WriteLine("imported");
            return queries;
        }

        public Dictionary<int, List<int>> CreateQueryLists(HashSet<int> queries)
        {
            Console.Write("Computing queries lists... ");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var queriesWithUrlsTermsMapped = new Dictionary<int, List<int>>();

            //foreach top 100 url and top 100 term
            for (int i = 0; i < _topUrlsAndTermsQueries.Length; i++)
            {
                //foreach query which occured in current url/term
                for (int j = 0; j < _topUrlsAndTermsQueries[i].Count; j++)
                {
                    var query = _topUrlsAndTermsQueries[i][j];
                    if (!queriesWithUrlsTermsMapped.ContainsKey(query))
                    {
                        queriesWithUrlsTermsMapped.Add(query, new List<int>());
                    }
                    queriesWithUrlsTermsMapped[query].Add(i);
                }
            }

            foreach (var query in queriesWithUrlsTermsMapped)
            {
                query.Value.Sort();
            }

            stopwatch.Stop();
            Console.WriteLine("took " + stopwatch.Elapsed.TotalSeconds + "s.");
            return queriesWithUrlsTermsMapped;
        }

        public string CreateInsertCmd()
        {
            var cmd = "COPY LOG (";
            for (int i = 0; i < 100; i++)
            {
                cmd += "url" + i +", ";
            }
            for (int i = 0; i < 100; i++)
            {
                cmd += "term" + i +", ";
            }
            //obciecie ostatniego przecinka
            cmd = cmd.Substring(0, cmd.Length - 3);
            cmd += ") FROM STDIN";
            return cmd;
        }

        public bool export()
        {
            NpgsqlCommand cmd = new NpgsqlCommand(CreateInsertCmd(), connection);
            NpgsqlCopySerializer serializer = new NpgsqlCopySerializer(connection);
            NpgsqlCopyIn copyIn = new NpgsqlCopyIn(cmd, connection, serializer.ToStream);
            const int FLUSH_ROWS = 200000;
            copyIn.Start();
            var linecounter = 0;
            foreach (var queryList in _queryLists)
            {
                for (int i = 0; i < 200; i++)
                {
                    serializer.AddBool(queryList.Value.Contains(i));
                }
                serializer.EndRow();

                if(linecounter++ %FLUSH_ROWS==0) serializer.Flush();
            }

            serializer.Flush();
            serializer.Close();
            copyIn.End();
            return true;
        }

        public string[] CreateColumnNamesArray()
        {
            var res = new string[200];
            for (int i = 0; i < 100; i++)
            {
                res[i] = "url" + i;
            }
            for (int i = 0; i < 100; i++)
            {
                res[i+100] = "term" + i;
            }
            return res;
        }

        public string[] CreateColumnTypesArray()
        {
            var res = new string[200];
            for (int i = 0; i < 200; i++)
            {
                res[i] = "boolean";
            }
            return res;
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }

            connection.Close();
        }

        private void rollback()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
        }
    }
}
