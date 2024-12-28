using DivinitySoftworks.Workers.Mortgage.Data;

using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.Requests;

/// <summary>
/// Represents a request model for mortgage interest data. It encapsulates the bank's information,
/// mortgage interest date, name, and associated debt market ratios.
/// </summary>
public sealed record MortgageInterestRequest {
    /// <summary>
    /// Initializes a new instance of the <see cref="MortgageInterestRequest"/> class 
    /// by copying values from a <see cref="MortgageInterest"/> object.
    /// </summary>
    /// <param name="mortgageInterest">The <see cref="MortgageInterest"/> object containing the bank ID, date, name, and debt market ratios.</param>
    public MortgageInterestRequest(MortgageInterest mortgageInterest) {
        BankId = mortgageInterest.BankId;
        Date = mortgageInterest.Date;
        Name = mortgageInterest.Name;
        DebtMarketRatios = mortgageInterest.DebtMarketRatios.ConvertAll(r => new DebtMarketRatioRequest(r));
    }

    /// <summary>
    /// Gets the unique identifier of the bank.
    /// </summary>
    [JsonPropertyName("bank_id")]
    public Guid BankId { get; init; }

    /// <summary>
    /// Gets the date of the mortgage interest in Unix time format.
    /// </summary>
    [JsonPropertyName("date")]
    public long Date { get; init; } = default!;

    /// <summary>
    /// Gets the name associated with the mortgage interest data.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the list of debt market ratios associated with this mortgage interest.
    /// </summary>
    [JsonPropertyName("ratios")]
    public List<DebtMarketRatioRequest> DebtMarketRatios { get; init; }
}
