using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Contracts.Requests;

/// <summary>
/// Represents a request to create or update mortgage interest information, including the bank ID, date, name, and associated debt market ratios.
/// </summary>
public sealed record MortgageInterestRequest {
    /// <summary>
    /// Gets or sets the bank identifier for the mortgage interest request.
    /// </summary>
    [JsonPropertyName("bank_id")]
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the date for the mortgage interest request.
    /// </summary>
    [JsonPropertyName("date")]
    public long Date { get; init; } = default!;

    /// <summary>
    /// Gets or sets the name associated with the mortgage interest request.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of debt market ratios included in the mortgage interest request.
    /// </summary>
    [JsonPropertyName("ratios")]
    public List<DebtMarketRatioRequest> DebtMarketRatios { get; set; } = new List<DebtMarketRatioRequest>();
}