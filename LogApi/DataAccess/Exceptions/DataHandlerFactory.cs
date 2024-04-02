using LogApi.DataAccess.Exceptions;
using LogApi.DataAccess.Exceptions.Databases;

public class DataHandlerFactory
{
    private readonly IConfiguration _configuration;

    public DataHandlerFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDataHandler CreateDataHandler()
    {
        var databaseName = _configuration.GetValue<string>("DatabaseName");

        switch (databaseName)
        {
            case "SQLite":
                var sqliteConnectionString = _configuration.GetConnectionString("SqliteConnection");
                return new DataHandlerSQLite(sqliteConnectionString);
            case "Microsoft Sql Server":
                var sqlServerConnectionString = _configuration.GetConnectionString("SsmsConnection");
                return new DataHandlerSsms(sqlServerConnectionString);
            case "File":
                var baseLogDirectory = _configuration.GetValue<string>("Logging:BaseLogDirectory");
                var logFileName = _configuration.GetValue<string>("Logging:LogFileName");
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseLogDirectory, logFileName);
                return new FileExceptionHandler(logFilePath);
            case "Postgres":
                var PostgresConnectionString = _configuration.GetConnectionString("PostgresConnection");
                return new DataHandlerPostgres(PostgresConnectionString);
            case "Mongo":
                var MongoConnectionString = _configuration.GetConnectionString("MongoConnection");
                return new DataHandlerMongo(MongoConnectionString);
            case "Mysql":
                var MysqlConnectionString = _configuration.GetConnectionString("MysqlConnection");
                return new DataHandlerMysql(MysqlConnectionString);
            default:
                throw new ArgumentException("Unsupported database name: " + databaseName);
        }
    }
}
