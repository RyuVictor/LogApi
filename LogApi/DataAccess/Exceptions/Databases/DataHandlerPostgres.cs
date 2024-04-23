using LogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace LogApi.DataAccess.Exceptions.Databases
{
    public class DataHandlerPostgres : AesEncryption.EncrytionLogic, IDataHandler
    {
        private readonly string _connectionString;
        public DataHandlerPostgres(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddException(MyException exception)
        {
            byte[] encryptedMessage = @Encrypt(Encoding.UTF8.GetBytes(exception.Message));
            byte[] encryptedStackTrace = @Encrypt(Encoding.UTF8.GetBytes(exception.StackTrace));
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the application exists in the application table
                using (var checkCommand = new NpgsqlCommand("SELECT applicationId FROM application WHERE applicationName = @ApplicationName", connection))
                {
                    checkCommand.Parameters.AddWithValue("@ApplicationName", exception.ApplicationName);
                    var existingAppId = checkCommand.ExecuteScalar();

                    if (existingAppId == null)
                    {
                        // Application does not exist, insert it into the application table
                        using (var insertAppCommand = new NpgsqlCommand("INSERT INTO application (applicationName) VALUES (@ApplicationName) RETURNING applicationId", connection))
                        {
                            insertAppCommand.Parameters.AddWithValue("@ApplicationName", exception.ApplicationName);
                            existingAppId = insertAppCommand.ExecuteScalar();
                        }
                    }

                    // Proceed with inserting the exception
                    using (var command = new NpgsqlCommand("INSERT INTO exceptions (statuscode, message, stacktrace, source, severity, applicationId) VALUES (@StatusCode, @Message, @StackTrace, @Source, @Severity, @ApplicationId)", connection))
                    {
                        command.Parameters.AddWithValue("@StatusCode", exception.StatusCode);
                        command.Parameters.AddWithValue("@Message", Convert.ToBase64String(encryptedMessage));
                        command.Parameters.AddWithValue("@StackTrace", Convert.ToBase64String(encryptedStackTrace));
                        command.Parameters.AddWithValue("@Source", exception.Source);
                        command.Parameters.AddWithValue("@Severity", exception.Severity);
                        command.Parameters.AddWithValue("@ApplicationId", existingAppId);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        public List<MyException> GetAllExceptions()
        {
            var exceptions = new List<MyException>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT e.id, e.statuscode, e.message, e.stacktrace, e.source, e.severity, a.applicationName, e.timestamp FROM Exceptions e INNER JOIN Application a ON e.applicationId = a.applicationId", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var exception = new MyException
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            StatusCode = Convert.ToInt32(reader["statuscode"]),
                            Message = Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(reader["message"].ToString()))),
                            StackTrace = Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(reader["stacktrace"].ToString()))),
                            Source = Convert.ToString(reader["source"]),
                            Severity = Convert.ToString(reader["severity"]),
                            ApplicationName = Convert.ToString(reader["applicationName"]),
                            Timestamp = Convert.ToDateTime(reader["timestamp"])
                        };
                        exceptions.Add(exception);
                    }
                }
            }

            return exceptions;
        }

        public void DeleteException(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("DELETE FROM Exceptions WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<GroupCount> GroupExceptionsByProperty(string propertyName)
        {
            var propertyCounts = new List<GroupCount>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = $"SELECT a.applicationname AS {propertyName}, COUNT(e.id) AS ExceptionCount FROM Exceptions e INNER JOIN Application a ON e.applicationId = a.applicationId GROUP BY a.applicationname";

                if (propertyName == "statuscode")
                {
                    query = $"SELECT e.statusCode AS {propertyName}, COUNT(e.id) AS ExceptionCount FROM Exceptions e GROUP BY e.statusCode";
                }
                else if (propertyName == "source")
                {
                    query = $"SELECT e.source AS {propertyName}, COUNT(e.id) AS ExceptionCount FROM Exceptions e GROUP BY e.source";
                }

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var propertyCount = new GroupCount
                        {
                            GroupName = Convert.ToString(reader[propertyName]),
                            Count = Convert.ToInt32(reader["ExceptionCount"])
                        };
                        propertyCounts.Add(propertyCount);
                    }
                }
            }

            return propertyCounts;
        }
        public List<MyException> FilterExceptionsByProperty(List<MyException> exceptions,string propertyName, string propertyValue)
        {
            if(exceptions.Count == 0) exceptions = GetAllExceptions();
            List<MyException> filteredExceptions = new List<MyException>();

            foreach (var exception in exceptions)
            {
                    if (MatchPropertyValue(exception, propertyName, propertyValue))
                    {
                        filteredExceptions.Add(exception);
                    }           
            }
            Console.WriteLine(filteredExceptions.Count+" this is the count");
            return filteredExceptions;
        }
        private bool MatchPropertyValue(MyException exception, string propertyName, string propertyValue)
        {
            switch (propertyName.ToLower())
            {
                case "statuscode":
                    return exception.StatusCode.ToString().Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "message":
                    return exception.Message.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "stacktrace":
                    return exception.StackTrace.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "source":
                    return exception.Source.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "severity":
                    return exception.Severity.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "timestamp":
                    return exception.Timestamp.ToString().Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                case "applicationname":
                    return exception.ApplicationName.Equals(propertyValue, StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }
        private int GetApplicationId(string applicationName)
        {
            int applicationId = -1; // Default value if applicationId is not found

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT applicationId FROM Application WHERE applicationName = @ApplicationName", connection))
                {
                    command.Parameters.AddWithValue("ApplicationName", applicationName);

                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        applicationId = Convert.ToInt32(result);
                    }
                }
            }

            return applicationId;
        }
        public List<MyException> GetRecentExceptions()
        {
            List<MyException> exceptions = GetAllExceptions();
            List<MyException> recentExceptions = exceptions.OrderBy(e => e.Timestamp).Take(10).ToList();
            return recentExceptions;
        }
    }
 }
