using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dapper;
using Npgsql;
using VTC.Common;

namespace VTC.db
{
    public static class DatabaseManager
    {
        public static DbConnection OpenConnection(UserConfig config)
        {
            if (config.SQLite)
            {
                //Check if database exists
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                const string filename = "VTC.sqlite3";
                var filePath = Path.Combine(appDataFolder, "VTC", filename);
                var cs = "PRAGMA foreign_keys = ON; URI=file:" + filePath;

                var fileExists = File.Exists(filePath);

                var dbConnection = new SQLiteConnection(cs);
                dbConnection.Open();

                if (!fileExists)
                {
                    //If the SQLite file did not exist previously, we can 'reset' the database to create it in a clean state.
                    ResetDatabase(dbConnection, config);
                }

                return dbConnection;
            }
            else
            {
                var cs = $"Host={config.DatabaseUrl};Port={config.DatabasePort};Username={config.Username};Password={config.Password};Database={config.DatabaseName}";
                var dbConnection = new NpgsqlConnection(cs);
                dbConnection.Open();
                return dbConnection;
            }
        }

        public static void CreateTables(DbConnection connection, UserConfig config)
        {
            if (!config.SQLite)
            {
                //TODO: Create sequences if using postgres
            }

            CreateJobTable(connection, config);
            CreateMovementTable(connection, config);
            CreateStateEstimateTable(connection, config);
        }

        public static void CreateJobTable(DbConnection connection, UserConfig config)
        {
            try
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = config.SQLite ? Resource1.CreateJobTableSQLite : Resource1.CreateJobTablePostgresql;
                cmd.ExecuteNonQuery();
            }
            catch (PostgresException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        public static void CreateMovementTable(DbConnection connection, UserConfig config)
        {
            try
            { 
                var cmd = connection.CreateCommand();
                cmd.CommandText = config.SQLite? Resource1.CreateMovementTableSQLite : Resource1.CreateMovementTableSQLPostgresql;
                cmd.ExecuteNonQuery();
            }
            catch (PostgresException ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public static void CreateStateEstimateTable(DbConnection connection, UserConfig config)
        {
            try
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = config.SQLite? Resource1.CreateStateEstimateTableSQLite : Resource1.CreateStateEstimateTableSQLPostgresql;
                cmd.ExecuteNonQuery();
            }
            catch (PostgresException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static void ResetDatabase(DbConnection connection, UserConfig config)
        {
            var cmdDropStateEstimateTable = connection.CreateCommand();
            cmdDropStateEstimateTable.CommandText = "DROP TABLE IF EXISTS stateestimate";
            cmdDropStateEstimateTable.ExecuteNonQuery();

            var cmdDropMovementTable = connection.CreateCommand();
            cmdDropMovementTable.CommandText = "DROP TABLE IF EXISTS movement";
            cmdDropMovementTable.ExecuteNonQuery();

            var cmdDropJobTable = connection.CreateCommand();
            cmdDropJobTable.CommandText = "DROP TABLE IF EXISTS job";
            cmdDropJobTable.ExecuteNonQuery();

            //TODO: Drop sequences if using postgres

            CreateTables(connection, config);
        }

        public static List<Movement> GetMovementsByJob(DbConnection connection, long jobId)
        {
            var movements = connection.Query<Movement>($"SELECT * FROM movement WHERE jobid = {jobId}").ToList();
            return movements;
        }
    }
}
