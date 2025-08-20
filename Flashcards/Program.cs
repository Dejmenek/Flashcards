using Flashcards;
using Flashcards.Controllers;
using Flashcards.DataAccess;
using Flashcards.DataAccess.Interfaces;
using Flashcards.DataAccess.Repositories;
using Flashcards.Services;
using Flashcards.Services.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Sinks.MSSqlServer;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("Default");

Log.Logger = new LoggerConfiguration()
    .WriteTo
    .MSSqlServer(
        connectionString: connectionString,
        new MSSqlServerSinkOptions
        {
            TableName = "Logs"
        }
    ).CreateLogger();

var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddConfiguration(config);
        })
        .ConfigureServices((hostContext, services) =>
        {
            var connectionString = config.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("The connection string 'Default' is not configured or is empty. Please check your appsettings.json file.");
            }

            services.AddLogging(builder =>
            {
                builder.AddSerilog(Log.Logger);
            });

            services.AddSingleton<IUserInteractionService, UserInteractionService>();
            services.AddSingleton<IConsoleService, ConsoleService>();
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
        .UseSerilog()
        .Build();

var dataContext = new DataContext(config);
await dataContext.Init();

var menu = host.Services.GetRequiredService<Menu>();
await menu.RunAsync();