using LogApi.DataAccess.Exceptions.Databases;
using LogApi.DataAccess.Users;
using LogApi.Middleware;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DataHandlerSsms class with connection string
var appBaseDirectory = AppContext.BaseDirectory;
builder.Services.AddSingleton<DataHandlerFactory>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new DataHandlerFactory(configuration);
});
builder.Services.AddSingleton<UserHandlerFactory>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new UserHandlerFactory(configuration);
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

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
