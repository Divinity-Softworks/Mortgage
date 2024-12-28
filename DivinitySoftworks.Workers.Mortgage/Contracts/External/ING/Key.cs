using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;

/// <summary>
/// Represents a key containing a name, data type, and a value.
/// </summary>
public sealed class Key
{
    /// <summary>
    /// Gets or sets the name of the key.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the data type of the key.
    /// </summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value associated with the key.
    /// </summary>
    [JsonPropertyName("value")]
    public Value Value { get; set; } = default!;
}
