using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Contracts.Requests;

/// <summary>
/// Represents a request for a debt market ratio, including the ratio, years, and interest rate.
/// </summary>
public sealed record DebtMarketRatioRequest {
    /// <summary>
    /// Gets or sets the debt market ratio percentage.
    /// </summary>
    [JsonPropertyName("ratio")]
    public int Ratio { get; set; }

    /// <summary>
    /// Gets or sets the number of years associated with the debt market ratio.
    /// </summary>
    [JsonPropertyName("years")]
    public int Years { get; set; }

    /// <summary>
    /// Gets or sets the interest rate associated with the debt market ratio.
    /// </summary>
    [JsonPropertyName("interest")]
    public decimal Interest { get; set; }
}
