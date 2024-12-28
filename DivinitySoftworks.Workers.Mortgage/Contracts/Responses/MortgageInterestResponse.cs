using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.Responses;

/// <summary>
/// Represents a response model for mortgage interest data, which includes the bank's information, 
/// the date of the mortgage interest, the bank's name, and the associated debt market ratios.
/// </summary>
public sealed record MortgageInterestResponse {
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
    public List<DebtMarketRatioResponse> DebtMarketRatios { get; init; } = [];
}
