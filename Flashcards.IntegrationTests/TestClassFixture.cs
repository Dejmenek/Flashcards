using Flashcards.Controllers;
using Flashcards.DataAccess;
using Flashcards.DataAccess.Interfaces;
using Flashcards.DataAccess.Repositories;
using Flashcards.Services;
using Flashcards.Services.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NSubstitute;

using Serilog;
using Serilog.Sinks.MSSqlServer;

using Testcontainers.MsSql;

namespace Flashcards.IntegrationTests;
public class TestClassFixture : IAsyncLifetime
{
    private MsSqlContainer _dbContainer;
    public IHost TestHost { get; set; }

    public async Task InitializeAsync()
    {
        var config = await SetupDatabaseAndConfigurationAsync();

        Log.Logger = new LoggerConfiguration()
            .WriteTo
            .MSSqlServer(
                connectionString: config.GetConnectionString("Master"),
                new MSSqlServerSinkOptions
                {
                    TableName = "Logs"
                }
            ).CreateLogger();

        TestHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddConfiguration(config);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("The connection string 'Default' is not configured or is empty. Please check your appsettings.json file.");
                }

                var userInteractionSubstitute = Substitute.For<IUserInteractionService>();
                var consoleServiceSubstitute = Substitute.For<IConsoleService>();

                services.AddLogging(builder =>
                {
                    builder.AddSerilog(Log.Logger);
                });

                services.AddSingleton(userInteractionSubstitute);
                services.AddSingleton(consoleServiceSubstitute);
                services.AddScoped<ICardsRepository, CardsRepository>();
                services.AddScoped<IStacksRepository, StacksRepository>();
                services.AddScoped<IStudySessionsRepository, StudySessionsRepository>();
                services.AddScoped<IStudySessionsService, StudySessionsService>();
                services.AddScoped<IStacksService, StacksService>();
                services.AddSingleton<ICardsService, CardsService>();
                services.AddSingleton<StacksController>();
                services.AddSingleton<StudySessionsController>();
                services.AddSingleton<CardsController>();
                services.AddScoped<Menu>();
            })
            .Build();

        var context = new DataContext(config);
        await context.Init();
    }

    private async Task<IConfiguration> SetupDatabaseAndConfigurationAsync()
    {
        var isCI = Environment.GetEnvironmentVariable("CI") is not null;
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "YourStrong!Passw0rd";

        return isCI
            ? CreateCIConfiguration(password)
            : await CreateLocalConfigurationAsync(password);
    }

    private static IConfiguration CreateCIConfiguration(string password)
    {
        var (defaultConn, masterConn) = CreateConnectionStrings("sqlserver", 1433, password);

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Default", defaultConn },
                { "ConnectionStrings:Master", masterConn }
            })
            .AddEnvironmentVariables()
            .Build();
    }

    private async Task<IConfiguration> CreateLocalConfigurationAsync(string password)
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(1433, true)
            .WithPassword(password)
            .Build();

        await _dbContainer.StartAsync();
        var port = _dbContainer.GetMappedPublicPort(1433);
        var (defaultConn, masterConn) = CreateConnectionStrings("localhost", port, password);

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Default", defaultConn },
                { "ConnectionStrings:Master", masterConn }
            })
            .AddEnvironmentVariables()
            .Build();
    }

    private static (string Default, string Master) CreateConnectionStrings(string server, int port, string password)
        => ($"Server={server},{port};Database=Flashcards;User Id=sa;Password={password};TrustServerCertificate=True;",
            $"Server={server},{port};Database=master;User Id=sa;Password={password};TrustServerCertificate=True;");


    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        TestHost?.Dispose();
    }
}