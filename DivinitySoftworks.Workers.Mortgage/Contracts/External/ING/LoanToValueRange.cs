using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a loan-to-value range.
/// </summary>
public class LoanToValueRange {
    /// <summary>
    /// Gets or sets the interval for the loan-to-value range.
    /// </summary>
    [JsonPropertyName("ltvInterval")]
    public Interval? Interval { get; set; }

    /// <summary>
    /// Gets or sets the interest rate for the loan-to-value range.
    /// </summary>
    [JsonPropertyName("interestRate")]
    public float InterestRate { get; set; }
}
