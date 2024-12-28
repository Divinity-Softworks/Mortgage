using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.Responses;

/// <summary>
/// Represents a response model for a debt market ratio, containing the ratio, number of years, and interest rate.
/// </summary>
public sealed record DebtMarketRatioResponse {
    /// <summary>
    /// Gets the debt-to-market ratio.
    /// </summary>
    [JsonPropertyName("ratio")]
    public int Ratio { get; init; }

    /// <summary>
    /// Gets the number of years associated with the debt market ratio.
    /// </summary>
    [JsonPropertyName("years")]
    public int Years { get; init; }

    /// <summary>
    /// Gets the interest rate applied to the debt market ratio.
    /// </summary>
    [JsonPropertyName("interest")]
    public decimal Interest { get; init; }
}
