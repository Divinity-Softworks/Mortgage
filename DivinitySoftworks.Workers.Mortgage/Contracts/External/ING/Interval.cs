using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents an interval with inclusive and exclusive bounds.
/// </summary>
public class Interval {
    /// <summary>
    /// Gets or sets the upper bound of the interval (inclusive).
    /// </summary>
    [JsonPropertyName("toIncluding")]
    public int? ToIncluding { get; set; }

    /// <summary>
    /// Gets or sets the lower bound of the interval (exclusive).
    /// </summary>
    [JsonPropertyName("fromExcluding")]
    public int? FromExcluding { get; set; }
}
