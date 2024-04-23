using LogApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SQLite;

namespace LogApi.DataAccess.Exceptions.Databases
{
    public class DataHandlerSQLite : IDataHandler
    {
        private readonly string _connectionString;

        public DataHandlerSQLite(string connectionString)
        {
            _connectionString = connectionString;
           // InitializeDatabase(); // Ensure the database and table are created
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Check if Exceptions table exists
                using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Exceptions'", connection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        // Create the Exceptions table
                        using (var createExceptionsTableCommand = new SQLiteCommand(
                            "CREATE TABLE Exceptions (" +
                            "Id INTEGER PRIMARY KEY," +
                            "StatusCode INTEGER," +
                            "Message TEXT," +
                            "StackTrace TEXT," +
                            "Source TEXT," +
                            "Severity TEXT," +
                            "ApplicationId INTEGER," + // Foreign key to Application table
                            "Timestamp TEXT, " + // Add a comma here
                            "FOREIGN KEY(ApplicationId) REFERENCES Application(ApplicationId))", connection))
                        {
                            createExceptionsTableCommand.ExecuteNonQuery();
                        }
                    }
                }

                // Check if Application table exists
                using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Application'", connection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        // Create the Application table
                        using (var createApplicationTableCommand = new SQLiteCommand(
                            "CREATE TABLE Application (" +
                            "ApplicationId INTEGER PRIMARY KEY," +
                            "ApplicationName TEXT)", connection))
                        {
                            createApplicationTableCommand.ExecuteNonQuery();
                        }
                    }
                }

                // Add foreign key constraint to Exceptions table if it doesn't exist
                using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand("PRAGMA foreign_key_list(Exceptions)", connection))
                {
                    var reader = command.ExecuteReader();
                    bool hasForeignKey = false;
                    while (reader.Read())
                    {
                        string foreignKeyTable = reader["table"].ToString();
                        string foreignKeyColumn = reader["from"].ToString();
                        if (foreignKeyTable == "Application" && foreignKeyColumn == "ApplicationId")
                        {
                            hasForeignKey = true;
                            break;
                        }
                    }

                    if (!hasForeignKey)
                    {
                        using (var addForeignKeyCommand = new SQLiteCommand(
                            "ALTER TABLE Exceptions ADD COLUMN ApplicationId INTEGER REFERENCES Application(ApplicationId)", connection))
                        {
                            addForeignKeyCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void AddException(MyException exception)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Check if the application exists in the application table
                using (var checkCommand = new SQLiteCommand("SELECT applicationId FROM application WHERE applicationName = @ApplicationName", connection))
                {
                    checkCommand.Parameters.AddWithValue("@ApplicationName", exception.ApplicationName);
                    var existingAppId = checkCommand.ExecuteScalar();

                    if (existingAppId == null)
                    {
                        // Application does not exist, insert it into the application table
                        using (var insertAppCommand = new SQLiteCommand("INSERT INTO application (applicationName) VALUES (@ApplicationName); SELECT last_insert_rowid();", connection))
                        {
                            insertAppCommand.Parameters.AddWithValue("@ApplicationName", exception.ApplicationName);
                            existingAppId = insertAppCommand.ExecuteScalar();
                        }
                    }

                    // Proceed with inserting the exception
                    using (var command = new SQLiteCommand("INSERT INTO Exceptions (StatusCode, Message, StackTrace, Source, Severity, ApplicationId) VALUES (@StatusCode, @Message, @StackTrace, @Source, @Severity, @ApplicationId)", connection))
                    {
                        command.Parameters.AddWithValue("@StatusCode", exception.StatusCode);
                        command.Parameters.AddWithValue("@Message", exception.Message);
                        command.Parameters.AddWithValue("@StackTrace", exception.StackTrace);
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

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT e.Id, e.StatusCode, e.Message, e.StackTrace, e.Source, e.Severity,e.Timestamp, a.ApplicationName FROM Exceptions e INNER JOIN Application a ON e.ApplicationId = a.ApplicationId", connection))
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
                            Severity = Convert.ToString(reader["Severity"]),
                            ApplicationName = Convert.ToString(reader["ApplicationName"]),
                        };

                        if (!reader.IsDBNull("Timestamp"))
                        {
                            exception.Timestamp = Convert.ToDateTime(reader["Timestamp"]);
                        }

                        exceptions.Add(exception);
                    }
                }
            }

            return exceptions;
        }
        public void DeleteException(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand("DELETE FROM Exceptions WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<GroupCount> GroupExceptionsByProperty(string propertyName)
        {
            var propertyCounts = new List<GroupCount>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = $"SELECT a.applicationName AS {propertyName}, COUNT(e.Id) AS ExceptionCount FROM Exceptions e INNER JOIN Application a ON e.ApplicationId = a.ApplicationId GROUP BY a.applicationName";

                if (propertyName == "statuscode")
                {
                    query = $"SELECT e.StatusCode AS {propertyName}, COUNT(e.Id) AS ExceptionCount FROM Exceptions e GROUP BY e.StatusCode";
                }
                else if (propertyName == "source")
                {
                    query = $"SELECT e.Source AS {propertyName}, COUNT(e.Id) AS ExceptionCount FROM Exceptions e GROUP BY e.Source";
                }

                using (var command = new SQLiteCommand(query, connection))
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

        public List<MyException> FilterExceptionsByProperty(List<MyException> exceptions, string propertyName, string propertyValue)
        {
            if (exceptions.Count == 0) exceptions = GetAllExceptions();
            List<MyException> filteredExceptions = new List<MyException>();

            foreach (var exception in exceptions)
            {
                if (MatchPropertyValue(exception, propertyName, propertyValue))
                {
                    filteredExceptions.Add(exception);
                }
            }

            Console.WriteLine(filteredExceptions.Count + " this is the count");
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

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT ApplicationId FROM Application WHERE ApplicationName = @ApplicationName", connection))
                {
                    command.Parameters.AddWithValue("@ApplicationName", applicationName);

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
