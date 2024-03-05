using LogApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;



namespace LogApi.DataAccess
{
    public class DataHandler : IDataHandler
    {
        private readonly string _connectionString;

        public DataHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddException(MyException exception)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("INSERT INTO Exceptions (StatusCode, Message, StackTrace, Source) VALUES (@StatusCode, @Message, @StackTrace, @Source)", connection))
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

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT Id, StatusCode, Message, StackTrace, Source FROM Exceptions", connection))
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
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("DELETE FROM Exceptions WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}