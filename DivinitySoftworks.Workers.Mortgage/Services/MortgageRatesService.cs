using DivinitySoftworks.Workers.Mortgage.Clients;
using DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;
using DivinitySoftworks.Workers.Mortgage.Contracts.Responses;
using DivinitySoftworks.Workers.Mortgage.Data;

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
    /// <param name="interestRateCollection">The collection of key values containing mortgage interest data.</param>
    /// <returns>
    /// A <see cref="MortgageInterest"/> object if parsing is successful, or <c>null</c> if the required data is not found or unchanged.
    /// </returns>
    MortgageInterest? Parse(Guid bankId, string bankName, InterestRateCollection interestRateCollection);
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
    public MortgageInterest? Parse(Guid bankId, string bankName, InterestRateCollection interestRateCollection) {
        // Check if the interest rate for the product 'IN002' is found. 'IN002' is 'Annuïteiten'.
        FixedInterestRate? fixedInterestRate = interestRateCollection.FixedInterestRates.FirstOrDefault(fir => fir.ProductCode == "IN002");
        if (fixedInterestRate is null) {
            _logger.LogWarning("Unable to parse, the poduct code 'IN002' was not found in the Interest Rate Collection.");
            return null;
        }

        DateTime mortgageInterestDateTime = DateTime.UtcNow.Date;

        if (mortgageInterestDateTimes.TryGetValue(bankId, out DateTime? datetime) && datetime == mortgageInterestDateTime)
            return null;

        MortgageInterest mortgageInterest = new() {
            BankId = bankId,
            Name = bankName,
            Date = mortgageInterestDateTime.ToUnixTimeSeconds()
        };

        foreach (RevisionPeriod revisionPeriod in fixedInterestRate.RevisionPeriods) {
            foreach (LoanToValueRange loanToValueRange in revisionPeriod.LoanToValueRanges) {
                DebtMarketRatio debtMarketRatio = new() {
                    Years = revisionPeriod.Months / 12,
                    Interest = (decimal)loanToValueRange.InterestRate
                };

                if (loanToValueRange.Interval is null || loanToValueRange.Interval.ToIncluding is null)
                    continue;

                debtMarketRatio.Ratio = loanToValueRange.Interval.ToIncluding.Value;

                mortgageInterest.DebtMarketRatios.Add(debtMarketRatio);
            }
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
