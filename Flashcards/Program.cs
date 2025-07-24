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

var host = Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(hostConfig =>
        {
            hostConfig.SetBasePath(AppContext.BaseDirectory);
            hostConfig.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            hostConfig.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
            var connectionString = hostContext.Configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("The connection string 'Default' is not configured or is empty. Please check your appsettings.json file.");
            }

            services.AddSingleton<IUserInteractionService, UserInteractionService>();
            services.AddSingleton<IConsoleService, ConsoleService>();
            services.AddScoped<IFlashcardsRepository, FlashcardsRepository>();
            services.AddScoped<IStacksRepository, StacksRepository>();
            services.AddScoped<IStudySessionsRepository, StudySessionsRepository>();
            services.AddScoped<IStudySessionsService, StudySessionsService>();
            services.AddScoped<IStacksService, StacksService>();
            services.AddSingleton<IFlashcardsService, FlashcardsService>();
            services.AddSingleton<StacksController>();
            services.AddSingleton<StudySessionsController>();
            services.AddSingleton<FlashcardsController>();
            services.AddScoped<Menu>();
        })
        .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var dataContext = new DataContext(config);
await dataContext.Init();

var menu = host.Services.GetRequiredService<Menu>();
await menu.RunAsync();