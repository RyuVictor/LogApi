using LogApi.DataAccess;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DataHandlerSsms class with connection string
var appBaseDirectory = AppContext.BaseDirectory;
builder.Services.AddSingleton<DataHandlerSsms>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("SsmsConnection");
    return new DataHandlerSsms(connectionString);
});
builder.Services.AddSingleton<FileExceptionHandler>(provider =>
{
    var logDirectory = Path.Combine(appBaseDirectory, "Logs");
    Directory.CreateDirectory(logDirectory); // Ensure the directory exists
    var logFilePath = Path.Combine(logDirectory, "log.txt");
    return new FileExceptionHandler(logFilePath);
});
builder.Services.AddSingleton<DataHandlerSQLite>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("SqliteConnection");
    return new DataHandlerSQLite(connectionString);
});
builder.Services.AddSingleton<DataHandlerPostgress>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("PostgresConnection");
    return new DataHandlerPostgress(connectionString);
});
builder.Services.AddSingleton<DataHandlerFactory>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new DataHandlerFactory(configuration);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
