
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Lift.Models;

namespace Lift.Database
{
    public static class DatabaseHelper
    {
        private const string DatabaseName = "LiftDB";

        // SQL Server LocalDB connection
        private static readonly string ConnectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;
              Initial Catalog=LiftDB;
              Integrated Security=True;
              Encrypt=True;
              TrustServerCertificate=True;";

        // Master connection used to create LiftDB if it does not exist
        private static readonly string MasterConnectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;
              Initial Catalog=master;
              Integrated Security=True;
              Encrypt=True;
              TrustServerCertificate=True;";

        // Initialize database and table
        public static void InitializeDatabase()
        {
            try
            {
                // Step 1: Create database if it does not exist
                using (SqlConnection conn =
                    new SqlConnection(MasterConnectionString))
                {
                    conn.Open();

                    string createDatabaseSql = @"
                        IF DB_ID(N'LiftDB') IS NULL
                        BEGIN
                            CREATE DATABASE [LiftDB];
                        END";

                    using (SqlCommand cmd =
                        new SqlCommand(createDatabaseSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Step 2: Connect to LiftDB and create table
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string createTableSql = @"
                        IF OBJECT_ID('dbo.ElevatorLog', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.ElevatorLog
                            (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                ElevatorId INT NOT NULL,
                                [Action] NVARCHAR(200) NOT NULL,
                                Floor INT NOT NULL,
                                [Timestamp] DATETIME2 NOT NULL
                            );
                        END";

                    using (SqlCommand cmd =
                        new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Debug.WriteLine(
                    "LiftDB connected and initialized successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Database initialization failed: {ex}");
                throw;
            }
        }

        // Insert elevator activity
        public static void LogAction(
            int elevatorId, string action, int floor)
        {
            try
            {
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO dbo.ElevatorLog
                        (ElevatorId, [Action], Floor, [Timestamp])
                        VALUES
                        (@eid, @action, @floor, @ts)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@eid",
                            System.Data.SqlDbType.Int).Value = elevatorId;

                        cmd.Parameters.Add("@action",
                            System.Data.SqlDbType.NVarChar, 200).Value = action;

                        cmd.Parameters.Add("@floor",
                            System.Data.SqlDbType.Int).Value = floor;

                        cmd.Parameters.Add("@ts",
                            System.Data.SqlDbType.DateTime2).Value = DateTime.Now;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Log insertion failed: {ex}");
                throw;
            }
        }

        // Retrieve latest 200 elevator activities
        public static List<LogEntry> GetAllLogs()
        {
            var list = new List<LogEntry>();

            try
            {
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT TOP (200)
                            Id,
                            ElevatorId,
                            [Action],
                            Floor,
                            [Timestamp]
                        FROM dbo.ElevatorLog
                        ORDER BY Id DESC";

                    using (SqlCommand cmd =
                        new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new LogEntry
                            {
                                Id = reader.GetInt32(0),
                                ElevatorId = reader.GetInt32(1),
                                Action = reader.GetString(2),
                                Floor = reader.GetInt32(3),
                                Timestamp = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Reading logs failed: {ex}");
                throw;
            }

            return list;
        }

        // Delete all elevator activities
        public static void ClearLogs()
        {
            try
            {
                using (SqlConnection conn =
                    new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string sql = "DELETE FROM dbo.ElevatorLog";

                    using (SqlCommand cmd =
                        new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Clearing logs failed: {ex}");
                throw;
            }
        }
    }
}
