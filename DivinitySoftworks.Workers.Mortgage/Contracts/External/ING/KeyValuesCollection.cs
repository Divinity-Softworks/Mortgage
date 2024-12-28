using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a collection of key-value groups.
/// </summary>
public sealed class KeyValuesCollection {
    /// <summary>
    /// Gets or sets the array of key-value groups.
    /// </summary>
    [JsonPropertyName("groups")]
    public KeyValueGroup[] Groups { get; set; } = [];
}

