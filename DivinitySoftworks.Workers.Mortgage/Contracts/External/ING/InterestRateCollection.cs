using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a collection of interest rates.
/// </summary>
public class InterestRateCollection {
    /// <summary>
    /// Gets or sets the fixed interest rates.
    /// </summary>
    [JsonPropertyName("fixedInterestRates")]
    public FixedInterestRate[] FixedInterestRates { get; set; } = [];
}