using DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;
using DivinitySoftworks.Workers.Mortgage.Data;
using DivinitySoftworks.Workers.Mortgage.Services;
using System.Diagnostics;

namespace DivinitySoftworks.Workers.Mortgage;

/// <summary>
/// A background worker service that periodically fetches mortgage rate data 
/// from an external source (ING), processes it, and logs the results.
/// </summary>
/// <param name="serviceScopeFactory">The factory for creating service scopes, used to access required services.</param>
/// <param name="mortgageRatesService">The service responsible for parsing and processing mortgage rate data.</param>
/// <param name="logger">The logging service used to log information, warnings, and errors.</param>
public class Worker(IServiceScopeFactory serviceScopeFactory, IMortgageRatesService mortgageRatesService, ILogger<Worker> logger) : BackgroundService {
    readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    readonly IMortgageRatesService _mortgageRatesService = mortgageRatesService;
    readonly ILogger<Worker> _logger = logger;

    const int _maxTries = 5;

    readonly Dictionary<string, Guid> _bankIdentifiers = new() {
        { "ING", new Guid("E798B0C6-5065-4804-ABD1-C8C4761CB745") }
    };

    DateTime? _lastCheck = default;

    /// <summary>
    /// Periodically executes the background service to fetch, parse, and process mortgage rate data 
    /// from the ING website. Retries fetching data up to a maximum of 5 times if it fails.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token to stop the background service gracefully.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            if (_lastCheck?.Date == DateTime.UtcNow.Date) {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                continue;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            int tries = 0;
            try {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                IWebMonitorService webMonitorService = scope.ServiceProvider.GetRequiredService<IWebMonitorService>();

                KeyValuesCollection? value = null;
                while (tries < _maxTries) {
                    tries++;
                    value = null;

                    try {
                        _logger.LogInformation("Fetching from the web ({Tries} of {MaxTries}).", tries, _maxTries);
                        value = await webMonitorService.FetchAsync("https://www.ing.nl/particulier/hypotheek/actuele-hypotheekrente");
                    }
                    catch (Exception exception) {
                        _logger.LogWarning(exception, "Failed to fetch from the web ({Tries} of {MaxTries}).", tries, _maxTries);
                    }

                    if (value is not null)
                        break;
                }

                if (value is null) {
                    _logger.LogWarning("No result returned from the web.");
                    continue;
                }

                MortgageInterest? mortgageInterest = _mortgageRatesService.Parse(_bankIdentifiers["ING"], "ING", value);

                if (mortgageInterest is null) {
                    _logger.LogError("Unable to parse the result to a valid Mortgage Interest object.");
                    continue;
                }

                _logger.LogInformation("Result returned from the web for {Date} ({UnixTimeSeconds}), now saving this data...", mortgageInterest.Date.FromUnixTimeSeconds(), mortgageInterest.Date);

                await _mortgageRatesService.ProcessAsync(mortgageInterest);

                _logger.LogInformation("The data for {Date} ({UnixTimeSeconds}) was saved.", mortgageInterest.Date.FromUnixTimeSeconds(), mortgageInterest.Date);

                _lastCheck = DateTime.UtcNow;

                stopwatch.Stop();
                _logger.LogInformation("Done after {Elapsed} in {Tries} tries!", stopwatch.Elapsed, tries);
            }
            catch (Exception exception) {
                stopwatch.Stop();
                _logger.LogError(exception, "Failed to execute after {Elapsed} in {Tries} tries.", stopwatch.Elapsed, tries);
            }
            finally {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
