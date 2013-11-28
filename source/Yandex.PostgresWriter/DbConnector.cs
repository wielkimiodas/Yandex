using System;
using System.Data;
using Npgsql;

namespace Yandex.PostgresWriter
{
    public class DbConnector
    {
        private readonly string _server = "localhost";
        private readonly string _userId = "postgres";
        private readonly string _port = "5432";
        private readonly string _password = "password";
        private readonly string _database = "yandex";
        private NpgsqlConnection _connection;

        public DbConnector()
        {
        }

        public DbConnector(string server, string userId, string port, string password, string database)
        {
            this._server = server;
            this._userId = userId;
            this._port = port;
            this._password = password;
            this._database = database;
        }

        private void Connect()
        {
            var connstring = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", _server,
                _port, _userId, _password, _database);
            _connection = new NpgsqlConnection(connstring);
            _connection.Open();
        }

        private void Disconnect()
        {
            _connection.Close();
        }

        public DataSet ExecuteQuery(string query)
        {
            Connect();

            var dataAdapter = new NpgsqlDataAdapter(query, _connection);
            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            Disconnect();

            return dataSet;
        }
    }
}