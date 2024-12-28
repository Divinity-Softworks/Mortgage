using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a value containing a unit and a value string.
/// </summary>
public sealed class Value
{
    /// <summary>
    /// Gets or sets the unit of the value.
    /// </summary>
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value string.
    /// </summary>
    [JsonPropertyName("value")]
    public string ValueString { get; set; } = default!;
}
