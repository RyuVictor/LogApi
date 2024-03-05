using LogApi.DataAccess;
using Microsoft.Extensions.Configuration;

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
                var sqlServerConnectionString = _configuration.GetConnectionString("DefaultConnection");
                return new DataHandler(sqlServerConnectionString);
            case "File":
                var baseLogDirectory = _configuration.GetValue<string>("Logging:BaseLogDirectory");
                var logFileName = _configuration.GetValue<string>("Logging:LogFileName");
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseLogDirectory, logFileName);
                return new FileExceptionHandler(logFilePath);
            default:
                throw new ArgumentException("Unsupported database name: " + databaseName);
        }
    }
}
