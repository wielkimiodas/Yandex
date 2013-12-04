using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using System.Threading;

namespace Yandex.Transfer
{
    public class Transfer : IDisposable
    {
        /// <summary>
        /// Nazwa użytkownika w bazie danych.
        /// </summary>
        private const string user = "postgres";

        /// <summary>
        /// Katalog do przechowywania przetworzonych plików z danymi.
        /// </summary>
        private readonly string workDir;

        /// <summary>
        /// Nazwa schematu w bazie danych.
        /// </summary>
        private readonly String schemaName;

        /// <summary>
        /// Określa, czy należy za każdym razem tworzyć nowe pliki tymczasowe
        /// i na końcu przetwarzania je usuwać.
        /// </summary>
        private const bool removeTmpFiles = false;

        /// <summary>
        /// Limit nałożony na liczbę odczytywanych linii z pliku z danymi.
        /// Jeżeli nie większy od 0, odczytywane są wszystkie linie.
        /// </summary>
        private const int LINES_LIMIT = 0;

        private const int MAX_CMD_TIMEOUT = 2*3600;

        // Nazwy tabel/plików.
        private const string sessionTableName = "session";
        private const string queryTableName = "query";
        private const string queryTermTableName = "query_term";
        private const string queryUrlTableName = "query_url";
        private const string clickTableName = "click";
        //private const string clickTmpTableName = "click_tmp";
        //private const string urlTmpTableName = "url_tmp";
        private const string urlTableName = "url";

        private NpgsqlConnection connection = null;
        private NpgsqlTransaction transaction = null;

        private Dictionary<string, Tuple<string[], string[]>> tables =
            new Dictionary<string, Tuple<string[], string[]>>();

        private bool confirmRemoveSchema = false;

        private bool toQuery = true;
        private string allQueries = "";

        /// <summary>
        /// Nawiązuje połączenie z bazą danych (i ew rozpoczyna transakcję).
        /// Tworzy definicję wszystkich tabel.
        /// </summary>
        /// <param name="connstring">Connstring używany do połączenia z bazą.</param>
        /// <param name="schemaName">Nazwa schematu, w którym mają zostać zapisane dane.</param>
        /// <param name="transactional">Określa, czy cały proces ma być wykonany transakcyjnie.</param>
        public Transfer(String connstring, String schemaName, String workDir, bool transactional = false)
        {
            this.schemaName = schemaName;
            this.workDir = workDir;

            connection = new NpgsqlConnection(connstring);
            connection.Open();

            if (transactional)
                transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            tables.Add(sessionTableName, new Tuple<string[], string[]>(new string[]
            {
                "session_id", "day", "user_id"
            }, new string[]
            {
                "integer", "integer", "integer"
            }));

            tables.Add(queryTableName, new Tuple<string[], string[]>(new string[]
            {
                "q_id", "query_id", "session_id", "serpid", "time_passed", "is_test"
            }, new string[]
            {
                "integer", "integer", "integer", "integer", "integer", "boolean"
            }));

            tables.Add(queryTermTableName, new Tuple<string[], string[]>(new string[]
            {
                "result_id", "term_id", "query_id"
            }, new string[]
            {
                "serial", "integer", "integer"
            }));

            tables.Add(queryUrlTableName, new Tuple<string[], string[]>(new string[]
            {
                "result_id", "url_id", "query_id"
            }, new string[]
            {
                "integer", "integer", "integer"
            }));

            tables.Add(clickTableName, new Tuple<string[], string[]>(new string[]
            {
                "click_id", "url_id", "q_id", "time_passed"
            }, new string[]
            {
                "serial", "integer", "integer", "integer"
            }));

            /*tables.Add(clickTmpTableName, new Tuple<string[], string[]>(new string[]
            {
                "session_id", "serpid", "url_id", "time_passed"
            }, new string[]
            {
                "integer", "integer", "integer", "integer"
            }));*/

            /*tables.Add(urlTmpTableName, new Tuple<string[], string[]>(new string[]
            {
                "url_id", "domain_id"
            }, new string[]
            {
                "integer", "integer"
            }));*/

            tables.Add(urlTableName, new Tuple<string[], string[]>(new string[]
            {
                "url_id", "domain_id"
            }, new string[]
            {
                "integer", "integer"
            }));
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }

            connection.Close();

            if (removeTmpFiles)
            {
                String[] filenames = new string[]
                {
                    sessionTableName, queryTableName, queryTermTableName, queryUrlTableName, clickTableName, urlTableName
                };
                foreach (String filename in filenames)
                    if (File.Exists(workDir + filename))
                        File.Delete(workDir + filename);
            }
        }

        private void rollback()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
        }

        private delegate bool BoolFunction();

        /// <summary>
        /// Przeprowadza wszystkie czynności związane z umieszczeniem daynch w bazie danych.
        /// </summary>
        /// <param name="filename">Nazwa pliku z danymi.</param>
        public void transfer(String filename)
        {
            var functions = new BoolFunction[]
            {
                createSchema, delegate { return rewriteData(filename); }, importData, createConstraints1, createIndexes1,
                copyTables, removeTables, createConstraints2, createIndexes2
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

            if (toQuery)
            {
                while (true)
                {
                    Thread th = new Thread((ThreadStart)delegate
                    {
                        System.Windows.Forms.Clipboard.SetText(allQueries);
                    });
                    th.SetApartmentState(ApartmentState.STA);
                    th.Start();

                    Console.Write("Queries copied to clipboard (y) ");
                    String line = Console.ReadLine();
                    if (line.ToLower().Equals("y"))
                        break;
                }
            }
        }

        /// <summary>
        /// Tworzy nowy schemat.
        /// Jeżeli schemat istnieje, usuwa go i tworzy nowy.
        /// </summary>
        private bool createSchema()
        {
            tic("Creating schema:");

            NpgsqlCommand cmd = new NpgsqlCommand(String.Format(
                "SELECT COUNT(*)<>0 FROM information_schema.schemata WHERE schema_name='{0}';", schemaName), connection);

            bool exist = Convert.ToBoolean(cmd.ExecuteScalar());

            DateTime stop = DateTime.Now;
            if (exist)
            {
                String reply = "Y";
                if (confirmRemoveSchema)
                {
                    Console.Write("SCHEMA ALREADY EXISTS. REMOVE IT (Y/N)? ");
                    reply = Console.ReadLine();
                }
                if (!reply.Equals("Y"))
                    return false;
                else
                {
                    cmd = new NpgsqlCommand(String.Format("DROP SCHEMA {0} CASCADE;", schemaName), connection);
                    cmd.ExecuteNonQuery();
                }
            }

            DateTime back = DateTime.Now;

            cmd = new NpgsqlCommand(String.Format("CREATE SCHEMA {0} AUTHORIZATION {1};", schemaName, user), connection);
            cmd.ExecuteNonQuery();

            toc();

            bool result = createTables();

            return result;
        }

        /// <summary>
        /// Tworzy tabele.
        /// </summary>
        private bool createTables()
        {
            tic("Creating tables:");

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

            toc();

            return true;
        }

        /// <summary>
        /// Przepisuje dane do plików odpowiadających za poszczególne tabele.
        /// </summary>
        /// <param name="filename">Plik z danymi.</param>
        private bool rewriteData(String filename)
        {
            tic("Rewriting data:");

            bool result = true;

            BinaryWriter session = null;
            BinaryWriter query = null;
            BinaryWriter queryTerm = null;
            BinaryWriter queryUrl = null;
            BinaryWriter click = null;
            BinaryWriter urlFile = null;

            session = new BinaryWriter(new FileStream(workDir + sessionTableName, FileMode.Create));
            query = new BinaryWriter(new FileStream(workDir + queryTableName, FileMode.Create));
            queryTerm = new BinaryWriter(new FileStream(workDir + queryTermTableName, FileMode.Create));
            queryUrl = new BinaryWriter(new FileStream(workDir + queryUrlTableName, FileMode.Create));
            click = new BinaryWriter(new FileStream(workDir + clickTableName, FileMode.Create));
            urlFile = new BinaryWriter(new FileStream(workDir + urlTableName, FileMode.Create));

            BinaryWriter[] writers = new BinaryWriter[] {session, query, queryTerm, queryUrl, click };

            {
                bool any = false;
                foreach (var writer in writers)
                    any = any || (writer != null);

                if (!any)
                {
                    toc();
                    return true;
                }
            }

            using (BufferedBinaryReader reader = new BufferedBinaryReader(filename))
            {
                int lineCounter = 0;
                int q_idCount = 0;
                int queryUrl_id = 0;

                /// rzutuje parę (url, serpid) na (result_id, q_id)
                Dictionary<Tuple<int, int>, int> urlInfo = new Dictionary<Tuple<int, int>, int>();
                int lastTime = 0;
                HashSet<int> urlsAdded = new HashSet<int>();
                HashSet<int> queriesAdded = new HashSet<int>();

                while (reader.PeekChar() > -1)
                {
                    lineCounter++;

                    if (LINES_LIMIT > 0 && LINES_LIMIT < lineCounter)
                        break;

                    byte type = reader.ReadByte();
                    if (type < 0 || type > 3)
                    {
                        Console.WriteLine("Incorrect file in line " + lineCounter);
                        result = false;
                        break;
                    }

                    int sessionId = reader.ReadInt32();

                    switch (type)
                    {
                        case 0:
                        {
                            int day = reader.ReadInt32();
                            int user = reader.ReadInt32();

                            session.Write((int) sessionId);
                            session.Write((int) day);
                            session.Write((int) user);
                            
                            urlInfo.Clear();
                            lastTime = 0;
                            break;
                        }
                        case 1:
                        case 2:
                        {
                            // TIME
                            int time = reader.ReadInt32();
                            // SERPID
                            int serpid = reader.ReadInt32();
                            // QUERYID
                            int queryId = reader.ReadInt32();

                            int q_id = q_idCount++;

                            query.Write((int) q_id);
                            query.Write((int) queryId);
                            query.Write((int) sessionId);
                            query.Write((int) serpid);
                            query.Write((int) (time - lastTime));
                            lastTime = time;
                            query.Write((bool) (type == 2));

                            bool processQuery = !queriesAdded.Contains(queryId);
                            if (processQuery)
                                queriesAdded.Add(queryId);

                            for (int i = reader.ReadInt32(); i > 0; i--)
                            {
                                int term = reader.ReadInt32();

                                if (processQuery)
                                {
                                    queryTerm.Write((int)term);
                                    queryTerm.Write((int)queryId);
                                }
                            }

                            for (int i = reader.ReadInt32(); i > 0; i--)
                            {
                                int url = reader.ReadInt32();
                                int domain = reader.ReadInt32();

                                Tuple<int, int> tuple = new Tuple<int, int>(url, serpid);
                                if (!urlInfo.ContainsKey(tuple))
                                    urlInfo.Add(tuple, q_id);

                                if (processQuery)
                                {
                                    queryUrl.Write((int)queryUrl_id);
                                    queryUrl_id++;

                                    queryUrl.Write((int)url);
                                    queryUrl.Write((int)queryId);

                                    if (!urlsAdded.Contains(url))
                                    {
                                        urlFile.Write((int)url);
                                        urlFile.Write((int)domain);
                                        urlsAdded.Add(url);
                                    }
                                }
                            }
                            break;
                        }
                        case 3:
                        {
                            // TIME
                            int time = reader.ReadInt32();
                            // SERPID
                            int serpid = reader.ReadInt32();
                            // URL
                            int url = reader.ReadInt32();

                            int q_id = urlInfo[new Tuple<int, int>(url, serpid)];

                            click.Write((int)url);
                            click.Write((int)q_id);
                            click.Write((int)(time - lastTime));
                            lastTime = time;

                            break;
                        }
                    }
                }
            }

            foreach (BinaryWriter writer in writers)
                if (writer != null)
                    writer.Dispose();

            toc();

            return result;
        }

        /// <summary>
        /// Wrzuca dane do bazy danych.
        /// </summary>
        private bool importData()
        {
            tic("Importing data:", true, true);

            const int FLUSH_ROWS = 200000;

            String[] toImport = new String[]
            {
                sessionTableName, queryTableName, queryTermTableName, queryUrlTableName, clickTableName, urlTableName
            };

            foreach (String tableName in toImport)
            {
                tic(tableName, false, true);

                int[] types = getTableTypes(tableName);

                NpgsqlCommand cmd = new NpgsqlCommand(buildInsertCommand(tableName), connection);
                NpgsqlCopySerializer serializer = new NpgsqlCopySerializer(connection);
                NpgsqlCopyIn copyIn = new NpgsqlCopyIn(cmd, connection, serializer.ToStream);

                copyIn.Start();

                using (BufferedBinaryReader reader = new BufferedBinaryReader(workDir + tableName))
                {
                    int lineCounter = 0;

                    while (reader.PeekChar() > -1)
                    {
                        lineCounter++;

                        for (int i = 0; i < types.Length; i++)
                        {
                            if (types[i] == 0)
                            {
                                int value = reader.ReadInt32();
                                serializer.AddInt32(value);
                            }
                            if (types[i] == 1)
                            {
                                bool value = reader.ReadBool();
                                serializer.AddBool(value);
                            }
                        }

                        serializer.EndRow();

                        if ((lineCounter + 1)%FLUSH_ROWS == 0)
                            serializer.Flush();
                    }

                    Console.Write(String.Format("{0,-15}", String.Format("({0})", lineCounter)));
                }

                serializer.Flush();
                serializer.Close();
                copyIn.End();

                toc();
            }

            toc(true);

            return true;
        }

        /// <summary>
        /// Kopiuje dane z tabel tymczsowych do tabel finalnych.
        /// </summary>
        private bool copyTables()
        {
            /*{
                int minUrlId = 0;
                int maxUrlId = 0;
                tic("Getting min and max");
                using (
                    NpgsqlCommand command =
                        new NpgsqlCommand(
                            String.Format("SELECT MIN(url_id), MAX(url_id) FROM {0}.{1};", schemaName, urlTmpTableName),
                            connection))
                {
                    command.CommandTimeout = MAX_CMD_TIMEOUT;
                    using (var result = command.ExecuteReader())
                    {
                        if (!result.HasRows)
                            throw new Exception("Something went wrong");

                        result.Read();

                        minUrlId = Convert.ToInt32(result[0]);
                        maxUrlId = Convert.ToInt32(result[1]) + 1;
                    }
                }
                toc();

                Console.WriteLine(String.Format("Min value: {0}", minUrlId));
                Console.WriteLine(String.Format("Max value: {0}", maxUrlId));

                int minValue = minUrlId;

                tic(String.Format("Inserting into {0}:", urlTableName), true);

                for (int i = 0; i < 10; i++)
                {
                    int maxValue = (int) ((i + 1)/10.0*(maxUrlId - minUrlId + 1)) + minUrlId;
                    tic(String.Format("{0,2}/{1} ({2,-10} <= url_id < {3,10})",
                        new object[] {i + 1, 10, minValue, maxValue}));
                    using (
                        NpgsqlCommand cmd =
                            new NpgsqlCommand(
                                String.Format(
                                    "INSERT INTO {0}.{1} (url_id, domain_id) SELECT DISTINCT ON (url_id) url_id, domain_id FROM {0}.{2} WHERE url_id >= {3} AND url_id < {4};",
                                    new object[] {schemaName, urlTableName, urlTmpTableName, minValue, maxValue}),
                                connection))
                    {
                        cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                        cmd.ExecuteNonQuery();
                    }
                    toc();

                    minValue = maxValue;
                }
                toc(true);
            }*/

            /*{
                int minSessionId = 0;
                int maxSessionId = 0;

                tic("Getting min and max");
                using (
                    NpgsqlCommand command =
                        new NpgsqlCommand(
                            String.Format("SELECT MIN(session_id), MAX(session_id) FROM {0}.{1};", schemaName,
                                queryTableName), connection))
                {
                    command.CommandTimeout = MAX_CMD_TIMEOUT;
                    using (var result = command.ExecuteReader())
                    {
                        if (!result.HasRows)
                            throw new Exception("Something went wrong");

                        result.Read();

                        minSessionId = Convert.ToInt32(result[0]);
                        maxSessionId = Convert.ToInt32(result[1]) + 1;
                    }
                }
                toc();

                Console.WriteLine(String.Format("Min value: {0}", minSessionId));
                Console.WriteLine(String.Format("Max value: {0}", maxSessionId));

                int minValue = minSessionId;

                tic(String.Format("Inserting into {0}:", clickTableName), true);

                for (int i = 0; i < 10; i++)
                {
                    int maxValue = (int) ((i + 1)/10.0*(maxSessionId - minSessionId + 1)) + minSessionId;
                    tic(String.Format("{0,2}/{1} ({2,-10} <= url_id < {3,10})",
                        new object[] {i + 1, 10, minValue, maxValue}));
                    using (
                        NpgsqlCommand cmd =
                            new NpgsqlCommand(String.Format("INSERT INTO {0}.{1} (result_id, time_passed) " +
                                                            "SELECT u.result_id, c.time_passed FROM " +
                                                            "{0}.{2} u INNER JOIN {0}.{3} q ON u.q_id=q.q_id " +
                                                            "INNER JOIN {0}.{4} c ON c.session_id=q.session_id AND c.serpid=q.serpid AND c.url_id=u.url_id WHERE q.session_id >= {5} AND q.session_id < {6};",
                                new object[]
                                {
                                    schemaName, clickTableName, queryUrlTableName, queryTableName, clickTmpTableName,
                                    minValue, maxValue
                                }), connection))
                    {
                        cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                        cmd.ExecuteNonQuery();
                    }
                    toc();

                    minValue = maxValue;
                }

                toc();
            }*/

            return true;
        }

        private int[] getTableTypes(String tableName)
        {
            List<int> typesList = new List<int>();
            foreach (String type in tables[tableName].Item2)
            {
                switch (type)
                {
                    case "serial":
                        break;
                    case "integer":
                        typesList.Add(0);
                        break;
                    case "boolean":
                        typesList.Add(1);
                        break;
                    default:
                        throw new Exception(String.Format("Unknown type {0}.", type));
                }
            }

            return typesList.ToArray();
        }

        private String buildInsertCommand(String tableName)
        {
            String cmd = String.Empty;
            for (int i = 0; i < tables[tableName].Item1.Length; i++)
            {
                if (!tables[tableName].Item2[i].Equals("integer") && !tables[tableName].Item2[i].Equals("boolean"))
                    continue;
                String field = tables[tableName].Item1[i];
                if (cmd.Equals(String.Empty))
                    cmd = field;
                else
                    cmd += ", " + field;
            }

            cmd = String.Format("COPY {0}.{1} ( {2} ) FROM STDIN;", schemaName, tableName, cmd);

            return cmd;
        }

        /// <summary>
        /// Usuwa tabele tymczasowe.
        /// </summary>
        /// <returns></returns>
        private bool removeTables()
        {
            /*foreach (String table in new String[] {urlTmpTableName, clickTmpTableName})
            {
                tic(String.Format("Dropping table {0}:", table));

                using (
                    NpgsqlCommand cmd = new NpgsqlCommand(String.Format("DROP TABLE {0}.{1};", schemaName, table),
                        connection))
                {
                    cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                    cmd.ExecuteNonQuery();
                }

                toc();
            }*/

            return true;
        }

        /// <summary>
        /// Tworzy klucz główny.
        /// </summary>
        /// <param name="tableName">Nazwa tabeli.</param>
        /// <param name="columnName">Nazwa kolumny.</param>
        private void createPK(String tableName, String columnName)
        {
            tic(String.Format("Creating PK {0}:", tableName));

            String text = String.Format("ALTER TABLE {0}.{1} ADD CONSTRAINT {1}_pk PRIMARY KEY ({2});", schemaName,
                            tableName, columnName);
            if (toQuery)
                allQueries += text + Environment.NewLine;
            else
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(text, connection))
                {
                    cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                    cmd.ExecuteNonQuery();
                }
            }

            toc();
        }

        /// <summary>
        /// Tworzy klucz obcy.
        /// </summary>
        /// <param name="tableName">Nazwa tabeli z kluczem obcym.</param>
        /// <param name="columnName">Kolumna tabeli.</param>
        /// <param name="refTable">Tabela, do której odnosi się klucz obcy.</param>
        /// <param name="refColumn">Kolumna, do której odnosi się klucz obcy.</param>
        private void createFK(String tableName, String columnName, String refTable, String refColumn)
        {
            tic(String.Format("Creating FK {0}:", tableName));

            String text = String.Format("ALTER TABLE {0}.{1} ADD CONSTRAINT {1}_fk_{3} FOREIGN KEY ({2}) REFERENCES {0}.{3} ({4}) ON UPDATE NO ACTION ON DELETE NO ACTION;",
                        new object[] { schemaName, tableName, columnName, refTable, refColumn });

            if (toQuery)
                allQueries += text + Environment.NewLine;
            else
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(text, connection))
                {
                    cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                    cmd.ExecuteNonQuery();
                }
            }

            toc();
        }

        /// <summary>
        /// Tworzy indeks.
        /// </summary>
        /// <param name="tableName">Tabela.</param>
        /// <param name="columnsName">Kolumna, na której ma zostać założony indeks.</param>
        private void createIndex(String tableName, String columnsName)
        {
            createIndex(tableName, new String[] {columnsName});
        }

        /// <summary>
        /// Tworzy indeks na kilku kolumnach.
        /// </summary>
        /// <param name="tableName">Tabela.</param>
        /// <param name="columnsName">Wektor kolumn, na którym ma zostać założony indeks.</param>
        private void createIndex(String tableName, String[] columnsName)
        {
            String columnsNames = String.Empty;
            String colsNames = String.Empty;
            foreach (String c in columnsName)
                if (columnsNames.Equals(String.Empty))
                {
                    columnsNames = c;
                    colsNames = c;
                }
                else
                {
                    columnsNames += ", " + c;
                    colsNames += "_" + c;
                }

            tic(String.Format("Creating index {0} ({1}):", tableName, columnsNames));

            String text = String.Format("CREATE INDEX {1}_{3}_index ON {0}.{1} ({2} ASC NULLS LAST);",
                        new object[] { schemaName, tableName, columnsNames, colsNames });

            if (toQuery)
                allQueries += text + Environment.NewLine;
            else
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(text, connection))
                {
                    cmd.CommandTimeout = MAX_CMD_TIMEOUT;
                    cmd.ExecuteNonQuery();
                }
            }
            
            toc();
        }

        /// <summary>
        /// Tworzy ograniczenia nie mające negatywnego wpływu na czas wykonywania funkcji <see cref="copyTables"/>.
        /// </summary>
        private bool createConstraints1()
        {
            tic("Creating constraints 1:", true);

            tic("Primary keys:", true);
            createPK(sessionTableName, "session_id");
            createPK(queryTableName, "q_id");
            createPK(queryTermTableName, "result_id");
            createPK(queryUrlTableName, "result_id");
            toc(true);

            tic("Foreign keys:", true);
            createFK(queryTableName, "session_id", sessionTableName, "session_id");
            //createFK(queryTermTableName, "query_id", queryTableName, "query_id");
            //createFK(queryUrlTableName, "query_id", queryTableName, "query_id");
            toc(true);

            toc(true);

            return true;
        }

        /// <summary>
        /// Tworzy ograniczenia mające negatywy wpływ na czas wykonywania funkcji <see cref="copyTables"/>.
        /// </summary>
        private bool createConstraints2()
        {
            tic("Creating constraints 2:", true);

            tic("Primary keys:", true);
            createPK(clickTableName, "click_id");
            createPK(urlTableName, "url_id");
            toc(true);

            tic("Foreign keys:", true);
            createFK(clickTableName, "q_id", queryTableName, "q_id");
            //createFK(clickTableName, "url_id", urlTableName, "url_id");
            createFK(queryUrlTableName, "url_id", urlTableName, "url_id");
            toc(true);

            toc(true);

            return true;
        }

        /// <summary>
        /// Tworzy indeksy nie mające negatywnego wpływu na czas wykonywania funkcji <see cref="copyTables"/>.
        /// </summary>
        private bool createIndexes1()
        {
            tic("Creating indexes 1:", true);

            createIndex(sessionTableName, "session_id");

            createIndex(queryTableName, "q_id");
            createIndex(queryTableName, "query_id");
            
            createIndex(queryTermTableName, "result_id");
            createIndex(queryTermTableName, new String[] { "query_id" });

            createIndex(queryUrlTableName, "result_id");
            createIndex(queryUrlTableName, new String[] { "query_id" });

            toc(true);

            return true;
        }

        /// <summary>
        /// Tworzy indeksy mające negatywy wpływ na czas wykonywania funkcji <see cref="copyTables"/>.
        /// </summary>
        private bool createIndexes2()
        {
            tic("Creating indexes 2:", true);

            createIndex(clickTableName, "url_id");
            createIndex(clickTableName, "q_id");

            createIndex(urlTableName, "url_id");
            createIndex(urlTableName, "domain_id");

            createIndex(queryUrlTableName, "url_id");

            toc(true);

            return true;
        }

        private Stack<DateTime> dateTimes = new Stack<DateTime>();
        private String prefix = "";

        private void tic(String text, bool newLine = false, bool shorter = false)
        {
            dateTimes.Push(DateTime.Now);

            if (newLine)
            {
                Console.WriteLine(prefix + text);
                prefix += "  ";
            }
            else if (shorter)
            {
                text = prefix + text;
                text = text.Substring(0, Math.Min(text.Length, 38));
                Console.Write(String.Format("{0, -40}", text));
            }
            else
            {
                text = prefix + text;
                text = text.Substring(0, Math.Min(text.Length, 53));
                Console.Write(String.Format("{0, -55}", text));
            }
        }

        private void toc(bool newLine = false)
        {
            if (newLine)
                prefix = prefix.Substring(2);

            Console.WriteLine((newLine ? String.Format("{0, -55}", prefix + "Total:") : "") +
                              (DateTime.Now - dateTimes.Pop()));
        }
    }
}