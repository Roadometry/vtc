using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VTC.Common;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Npgsql;
using System.Data.SQLite;
using System.Globalization;

namespace VTC.Common
{
    public class MovementCount : Dictionary<Movement,long>
    {
        public long TotalCount()
        {
            long count = 0;
            foreach(var mc in this)
            {
                count += mc.Value;
            }
            return count;
        }
    }

    [DataContract]
    public class Movement : IComparable<Movement>
    {
        [DataMember] public readonly string Approach;

        [DataMember] public readonly string Exit;

        [DataMember] [JsonConverter(typeof(StringEnumConverter))]
        public readonly Turn TurnType;

        [DataMember] [JsonConverter(typeof(StringEnumConverter))]
        public readonly ObjectType TrafficObjectType;

        [DataMember] public StateEstimateList StateEstimates;

        [DataMember] public readonly DateTime Timestamp;

        [DataMember] public int FirstDetectionFrame;

        [DataMember] public bool Ignored;

        [DataMember] public long JobId;

        [DataMember] public bool Synthetic;

        [DataMember] public long Id;

        public Movement(string approach, string exit, Turn turn, ObjectType objectType, StateEstimateList stateEstimates, DateTime timeStamp, bool ignored, long job = 0)
        {
            Approach = approach;
            Exit = exit;
            TurnType = turn;
            TrafficObjectType = objectType;
            StateEstimates = stateEstimates;
            Timestamp = timeStamp;
            Ignored = ignored;
            JobId = job;
        }

        public Movement()
        {
        }

        public override string ToString()
        {
            var s = Approach + " to " + Exit + ", " + Enum.GetName(typeof(Turn),TurnType) + ", " + Enum.GetName(typeof(ObjectType),TrafficObjectType);
            return s;
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var typeThat = obj.GetType();
                if(typeThat != typeof(Movement) && !typeThat.IsSubclassOf(typeof(Movement)))
                    return false;
            }

            var other = (Movement) obj;
            return other != null && (other.Approach == Approach && other.Exit == Exit);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public int CompareTo(Movement that)
        {
            return string.Compare(ToString(), that.ToString(), StringComparison.Ordinal);
        }

        public double MissRatio()
        {
            var missRatio = (double) StateEstimates.Sum(se => se.TotalMissedDetections) / StateEstimates.Count();
            return missRatio;
        }

        public void Save(DbConnection dbConnection, UserConfig config)
        {
            try
            {
                var transaction = dbConnection.BeginTransaction();
                if (config.SQLite)
                {
                    var sqliteConnection = (SQLiteConnection) dbConnection;
                    var cmd = sqliteConnection.CreateCommand();
                    cmd.CommandText =
                        $"INSERT INTO movement(jobid,approach,exit,movementtype,trafficobjecttype,timestamp,synthetic,ignored) VALUES({JobId},'{Approach}','{Exit}','{TurnType}','{TrafficObjectType}','{Timestamp.ToString(CultureInfo.InvariantCulture)}',{(Synthetic ? 1 : 0)},{(Ignored ? 1 : 0)})";
                    cmd.Transaction = (SQLiteTransaction) transaction;
                    cmd.ExecuteNonQuery();
                    Id = sqliteConnection.LastInsertRowId;
                }
                else
                {
                    var cmd = dbConnection.CreateCommand();
                    cmd.CommandText =
                        $"INSERT INTO movement(jobid,approach,exit,turntype,trafficobjecttype,timestamp,synthetic,ignored) VALUES({JobId},'{Approach}','{Exit}','{TurnType}','{TrafficObjectType}','{Timestamp.ToString(CultureInfo.InvariantCulture)}',{Synthetic},{Ignored}) RETURNING id";
                    cmd.Transaction = transaction;
                    var result = cmd.ExecuteScalar();
                    Id = int.Parse(result.ToString());
                }

                foreach (var s in StateEstimates)
                {
                    s.MovementId = Id;
                    s.Save(dbConnection);
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Movement.Save:" + e.Message);
            }
        }
    }
}