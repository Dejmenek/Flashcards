using Flashcards;
using Flashcards.Controllers;
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
            hostConfig.SetBasePath(Directory.GetCurrentDirectory());
            hostConfig.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            var connectionString = hostContext.Configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("The connection string 'Default' is not configured or is empty. Please check your appsettings.json file.");
            }

            services.AddSingleton<UserInteractionService>();
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

var menu = host.Services.GetRequiredService<Menu>();
menu.Run();