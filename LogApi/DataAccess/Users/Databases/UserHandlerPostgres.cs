using LogApi.Models;
using Npgsql;

namespace LogApi.DataAccess.Users.Databases
{
    public class UserHandlerPostgres : IUserHandler
    {
        private readonly string _connectionString;
        public UserHandlerPostgres(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddUser(LoggedInUser user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the user exists in the Users table
                using (var checkCommand = new NpgsqlCommand("SELECT ID FROM LoggedInusers WHERE userPrincipalName = @UserPrincipalName", connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserPrincipalName", user.UserPrincipalName);
                    var existingUserId = checkCommand.ExecuteScalar();

                    if (existingUserId == null)
                    {
                        // User does not exist, insert it into the Users table
                        using (var insertUserCommand = new NpgsqlCommand("INSERT INTO LoggedInusers (displayName, jobTitle, userPrincipalName, MobileNo, Timestamp) VALUES (@DisplayName, @JobTitle, @UserPrincipalName, @MobileNo, @Timestamp) RETURNING ID", connection))
                        {
                            insertUserCommand.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                            insertUserCommand.Parameters.AddWithValue("@JobTitle", user.JobTitle);
                            insertUserCommand.Parameters.AddWithValue("@UserPrincipalName", user.UserPrincipalName);
                            insertUserCommand.Parameters.AddWithValue("@MobileNo", user.MobileNo);
                            insertUserCommand.Parameters.AddWithValue("@Timestamp", user.Timestamp);
                            existingUserId = insertUserCommand.ExecuteScalar();
                        }
                    }
                    else
                    {
                        // User exists, update the Timestamp column
                        using (var updateUserCommand = new NpgsqlCommand("UPDATE LoggedInusers SET Timestamp = @Timestamp WHERE ID = @ID", connection))
                        {
                            updateUserCommand.Parameters.AddWithValue("@ID", existingUserId);
                            updateUserCommand.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);
                            updateUserCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        public List<LoggedInUser> GetAllUsers()
        {
            var users = new List<LoggedInUser>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT ID, displayName, jobTitle, userPrincipalName, MobileNo, Timestamp FROM LoggedInusers", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new LoggedInUser
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            DisplayName = Convert.ToString(reader["displayName"]),
                            JobTitle = Convert.ToString(reader["jobTitle"]),
                            UserPrincipalName = Convert.ToString(reader["userPrincipalName"]),
                            MobileNo = Convert.ToString(reader["MobileNo"]),
                            Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                        };
                        users.Add(user);
                    }
                }
            }

            return users;
        }
        public void DeleteUser(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the user with the specified ID exists in the LoggedinUsers table
                using (var checkCommand = new NpgsqlCommand("SELECT ID FROM LoggedinUsers WHERE ID = @ID", connection))
                {
                    checkCommand.Parameters.AddWithValue("@ID", id);
                    var existingUserId = checkCommand.ExecuteScalar();

                    if (existingUserId != null)
                    {
                        // User exists, proceed with deleting from the LoggedinUsers table
                        using (var deleteCommand = new NpgsqlCommand("DELETE FROM LoggedinUsers WHERE ID = @ID", connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@ID", id);
                            deleteCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
