using DivinitySoftworks.Core.Web.Http.Extentions;
using DivinitySoftworks.Workers.Mortgage;
using DivinitySoftworks.Workers.Mortgage.Clients;
using DivinitySoftworks.Workers.Mortgage.Services;

using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;

#pragma warning disable CA1416 // Validate platform compatibility

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) => {
        configurationBuilder.AddJsonFile("appsettings.json", false, true);
    })
    .ConfigureLogging((hostBuilderContext, loggingBuilder) => {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"));

        try {
            EventLogSettings? eventLogSettings = hostBuilderContext.Configuration.GetSection("Logging:EventLog").Get<EventLogSettings>();
            if (eventLogSettings is null)
                loggingBuilder.AddEventLog();

            if (eventLogSettings is not null) {
                if (!EventLog.SourceExists(eventLogSettings.SourceName))
                    EventLog.CreateEventSource(eventLogSettings.SourceName, "Application");

                loggingBuilder.AddEventLog(logSettings => {
                    logSettings.Filter = eventLogSettings.Filter;
                    logSettings.LogName = eventLogSettings.LogName;
                    logSettings.MachineName = eventLogSettings.MachineName;
                    logSettings.SourceName = eventLogSettings.SourceName;
                });
            }
        }
        catch {
            loggingBuilder.AddEventLog();
        }
    })
    .ConfigureServices((hostBuilderContext, serviceCollection) => {
        // Use the extension method to add TokenService
        serviceCollection.AddTokenService(hostBuilderContext.Configuration);

        serviceCollection.AddHttpClient<IMortgageClient, MortgageClient>(client => {
            client.BaseAddress = new Uri("https://mortgage.divinity-softworks.com");
        });

        serviceCollection.AddTransient<IWebMonitorService, WebMonitorService>();
        serviceCollection.AddSingleton<IMortgageRatesService, MortgageRatesService>();

        serviceCollection.AddHostedService<Worker>();
    });

hostBuilder.Build().Run();


#pragma warning restore CA1416 // Validate platform compatibility