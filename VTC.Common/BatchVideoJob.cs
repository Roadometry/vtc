using System;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using Npgsql;
using VTC.Common.RegionConfig;
using System.Data.SQLite;

namespace VTC.Common
{
    public class BatchVideoJob
    {
        public string VideoPath;
        public RegionConfig.RegionConfig RegionConfiguration;
        public string GroundTruthPath;
        public DateTime StartDateTime;
        public long Id;

        public void Save(DbConnection dbConnection,UserConfig config)
        {
            try
            {
                if (config.SQLite)
                {
                    var sqliteConnection = (SQLiteConnection) dbConnection;
                    var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText =
                        $"INSERT INTO job(videopath,regionconfigurationname,groundtruthpath,timestamp) VALUES('{VideoPath}','{RegionConfiguration.Title}','{GroundTruthPath}','{DateTime.Now}')";
                    var result = cmd.ExecuteScalar();
                    Id = sqliteConnection.LastInsertRowId;
                }
                else
                {
                    var cmd = dbConnection.CreateCommand();
                    cmd.CommandText =
                        $"INSERT INTO job(videopath,regionconfigurationname,groundtruthpath,timestamp) VALUES('{VideoPath}','{RegionConfiguration.Title}','{GroundTruthPath}','{DateTime.Now}') RETURNING id";
                    var result = cmd.ExecuteScalar();
                    Id = int.Parse(result.ToString());
                }

                
            }
            catch (Exception e)
            {
                Debug.WriteLine("BatchVideoJob.Save:" + e.Message);
            }
        }
    }
}
