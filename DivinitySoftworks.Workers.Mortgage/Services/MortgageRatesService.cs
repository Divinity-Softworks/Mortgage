using DivinitySoftworks.Workers.Mortgage.Clients;
using DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;
using DivinitySoftworks.Workers.Mortgage.Contracts.Responses;
using DivinitySoftworks.Workers.Mortgage.Data;

using System.Globalization;
using System.Text.RegularExpressions;

namespace DivinitySoftworks.Workers.Mortgage.Services;

/// <summary>
/// Interface for processing mortgage rates from different banks.
/// </summary>
public interface IMortgageRatesService {
    /// <summary>
    /// Parses a collection of key values to extract mortgage interest data for a specified bank.
    /// </summary>
    /// <param name="bankId">The unique identifier for the bank.</param>
    /// <param name="bankName">The name of the bank.</param>
    /// <param name="keyValuesCollection">The collection of key values containing mortgage interest data.</param>
    /// <returns>
    /// A <see cref="MortgageInterest"/> object if parsing is successful, or <c>null</c> if the required data is not found or unchanged.
    /// </returns>
    MortgageInterest? Parse(Guid bankId, string bankName, KeyValuesCollection keyValuesCollection);
    /// <summary>
    /// Processes the mortgage interest by posting it to an external service asynchronously.
    /// If successful, updates the local cache of mortgage interest rates with the new date.
    /// </summary>
    /// <param name="mortgageInterest">The mortgage interest data to process.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ProcessAsync(MortgageInterest value);
}

/// <summary>
/// Service class responsible for processing and posting mortgage interest rates using an <see cref="IMortgageClient"/> 
/// and logging the results. This class handles the parsing of key value collections related to mortgage interest data 
/// and checks if the interest rates for a specific bank have been updated. If new rates are found, 
/// they are sent for processing and logged accordingly.
/// </summary>
/// <param name="mortgageClient">The client used to communicate with external mortgage rate services for posting mortgage interest data.</param>
/// <param name="logger">The logger instance used to log relevant information and any errors encountered during processing.</param>
public sealed class MortgageRatesService(IMortgageClient mortgageClient, ILogger<MortgageRatesService> logger) : IMortgageRatesService {
    readonly IMortgageClient _mortgageClient = mortgageClient;
    readonly ILogger<MortgageRatesService> _logger = logger;

    readonly Dictionary<Guid, DateTime?> mortgageInterestDateTimes = [];

    /// <inheritdoc/>
    public MortgageInterest? Parse(Guid bankId, string bankName, KeyValuesCollection keyValuesCollection) {
        KeyValueGroup? keyValueGroup = keyValuesCollection.Groups.FirstOrDefault(g => g.Name == "prijs.hyp.vast");
        if (keyValueGroup is null) {
            _logger.LogWarning("Unable to parse, the group 'prijs.hyp.vast' was not found in the Key Value Collection.");
            return null;
        }

        Key? dateKey = keyValueGroup.Keys.FirstOrDefault(k => k.Name == "annuitair.inclABK.ingangsdatum.dmmmmjjjj");
        if (dateKey is null) {
            _logger.LogWarning("Unable to parse, the key 'annuitair.inclABK.ingangsdatum.dmmmmjjjj' was not found in the Key Value Collection.");
            return null;
        }

        DateTime mortgageInterestDateTime = DateTime.ParseExact(dateKey.Value.ValueString, "d MMMM yyyy", new CultureInfo(dateKey.Value.Unit), DateTimeStyles.AssumeUniversal);

        if (mortgageInterestDateTimes.TryGetValue(bankId, out DateTime? datetime) && datetime == mortgageInterestDateTime)
            return null;

        MortgageInterest mortgageInterest = new() {
            BankId = bankId,
            Name = bankName,
            Date = mortgageInterestDateTime.ToUnixTimeSeconds()
        };

        string pattern = @"annuitair\.inclABK\.(\d{2})jaar\.tm(\d{3})\.perc";
        foreach (Key key in keyValueGroup.Keys.Where(k => k.Name.StartsWith("annuitair.inclABK.") && k.Name.EndsWith(".perc"))) {
            Match match = Regex.Match(key.Name, pattern);

            if (!match.Success) continue;

            mortgageInterest
                .DebtMarketRatios
                .Add(new DebtMarketRatio {
                    Years = int.Parse(match.Groups[1].Value),
                    Ratio = int.Parse(match.Groups[2].Value),
                    Interest = decimal.Parse(key.Value.ValueString.Replace("%", string.Empty), new CultureInfo(key.Value.Unit))
                });
        }

        return mortgageInterest;
    }

    /// <inheritdoc/>
    public async Task ProcessAsync(MortgageInterest mortgageInterest) {
        MortgageInterestResponse? mortgageInterestResponse = await _mortgageClient
            .PostInterestsAsync(mortgageInterest);

        if (mortgageInterestResponse is null)
            return;

        if (mortgageInterestDateTimes.ContainsKey(mortgageInterest.BankId))
            mortgageInterestDateTimes[mortgageInterest.BankId] = mortgageInterestResponse.Date.FromUnixTimeSeconds();
        else
            mortgageInterestDateTimes.Add(mortgageInterest.BankId, mortgageInterestResponse.Date.FromUnixTimeSeconds());

        return;
    }
}
