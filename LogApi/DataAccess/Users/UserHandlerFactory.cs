using LogApi.DataAccess.Users.Databases;

namespace LogApi.DataAccess.Users
{
    public class UserHandlerFactory
    {
        private readonly IConfiguration _configuration;

        public UserHandlerFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IUserHandler CreateUserHandler()
        {
            var databaseName = _configuration.GetValue<string>("DatabaseName");

            switch (databaseName)
            {
                case "Postgres":
                    var PostgresConnectionString = _configuration.GetConnectionString("PostgresConnection");
                    return new UserHandlerPostgres(PostgresConnectionString);
                case "Mysql":
                    var MysqlConnectionString = _configuration.GetConnectionString("MysqlConnection");
                    return new UserHandlerMysql(MysqlConnectionString);
                default:
                    throw new ArgumentException("Unsupported database name: " + databaseName);
            }
        }
    }
}
