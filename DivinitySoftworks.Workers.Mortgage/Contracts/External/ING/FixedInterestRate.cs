using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a fixed interest rate.
/// </summary>
public class FixedInterestRate {
    /// <summary>
    /// Gets or sets the product code.
    /// </summary>
    [JsonPropertyName("hdnProductCode")]
    public string ProductCode { get; set; } = default!;

    /// <summary>
    /// Gets or sets the revision periods.
    /// </summary>
    [JsonPropertyName("revisionPeriods")]
    public RevisionPeriod[] RevisionPeriods { get; set; } = [];
}
