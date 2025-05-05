using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a revision period.
/// </summary>
public class RevisionPeriod {
    /// <summary>
    /// Gets or sets the number of months in the revision period.
    /// </summary>
    [JsonPropertyName("revisionPeriodMonths")]
    public int Months { get; set; }

    /// <summary>
    /// Gets or sets the interest rate for NHG.
    /// </summary>
    [JsonPropertyName("interestRateForNhg")]
    public float InterestRateForNhg { get; set; }

    /// <summary>
    /// Gets or sets the loan-to-value ranges.
    /// </summary>
    [JsonPropertyName("loanToValueRanges")]
    public LoanToValueRange[] LoanToValueRanges { get; set; } = [];
}
