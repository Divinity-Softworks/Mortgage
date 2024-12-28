using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a key-value group containing a name and an array of keys.
/// </summary>
public sealed class KeyValueGroup
{
    /// <summary>
    /// Gets or sets the name of the key-value group.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the array of keys in the group.
    /// </summary>
    [JsonPropertyName("keys")]
    public Key[] Keys { get; set; } = [];
}
