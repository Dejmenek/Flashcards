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
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(1433, true)
            .WithPassword("YourStrong!Passw0rd")
            .Build();

        await _dbContainer.StartAsync();

        var mappedPort = _dbContainer.GetMappedPublicPort(1433);

        var defaultConnectionString = $"Server=localhost,{mappedPort};Database=Flashcards;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
        var masterConnectionString = $"Server=localhost,{mappedPort};Database=master;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Default", defaultConnectionString },
                { "ConnectionStrings:Master", masterConnectionString }
            })
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .WriteTo
            .MSSqlServer(
                connectionString: masterConnectionString,
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

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        TestHost?.Dispose();
    }
}