using LogApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite; 

namespace LogApi.DataAccess
{
    public class DataHandlerSQLite : IDataHandler
    {
        private readonly string _connectionString;

        public DataHandlerSQLite(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase(); // Ensure the database and table are created
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Create the Exceptions table if it doesn't exist
                using (var command = new SqliteCommand(
                    "CREATE TABLE IF NOT EXISTS Exceptions (" +
                    "Id INTEGER PRIMARY KEY," +
                    "StatusCode INTEGER," +
                    "Message TEXT," +
                    "StackTrace TEXT," +
                    "Source TEXT)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddException(MyException exception)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand("INSERT INTO Exceptions (StatusCode, Message, StackTrace, Source) VALUES (@StatusCode, @Message, @StackTrace, @Source)", connection))
                {
                    command.Parameters.AddWithValue("@StatusCode", exception.StatusCode);
                    command.Parameters.AddWithValue("@Message", exception.Message);
                    command.Parameters.AddWithValue("@StackTrace", exception.StackTrace);
                    command.Parameters.AddWithValue("@Source", exception.Source);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<MyException> GetAllExceptions()
        {
            var exceptions = new List<MyException>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand("SELECT Id, StatusCode, Message, StackTrace, Source FROM Exceptions", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var exception = new MyException
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            StatusCode = Convert.ToInt32(reader["StatusCode"]),
                            Message = Convert.ToString(reader["Message"]),
                            StackTrace = Convert.ToString(reader["StackTrace"]),
                            Source = Convert.ToString(reader["Source"]),
                            Timestamp = DateTime.Now,
                        };
                        exceptions.Add(exception);
                    }
                }
            }

            return exceptions;
        }

        public void DeleteException(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand("DELETE FROM Exceptions WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
