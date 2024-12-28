using DivinitySoftworks.AWS.Core.Data.DynamoDB;
using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Data;

/// <summary>
/// Represents the mortgage interest information, including the bank identifier, date, name, and debt market ratios.
/// </summary>
public sealed record MortgageInterest : DynamoDBRecord<string> {

    /// <summary>
    /// Gets the primary key (PK) for the mortgage interest, which is derived from the BankId.
    /// </summary>
    [JsonPropertyName("PK")]
    public override required string PK {
        get {
            if (IsLatest)
                return $"{BankId.ToString().ToUpper()}.$LATEST";
            return BankId.ToString().ToUpper();
        }
        set {
            IsLatest = value.Contains(".$LATEST");
            BankId = Guid.Parse(value.Replace(".$LATEST", string.Empty));
        }
    }

    /// <summary>
    /// Gets the sort key (SK) for the mortgage interest, which is derived from the Date.
    /// </summary>
    [JsonIgnore]
    public long SK => Date;

    /// <summary>
    /// Gets or sets the bank identifier. This serves as the primary key (PK) when serialized.
    /// </summary>
    [JsonIgnore]
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the date of the mortgage interest record. This serves as the sort key (SK) when serialized.
    /// </summary>
    [JsonPropertyName("SK")]
    public long Date { get; init; } = default!;

    /// <summary>
    /// Gets or sets the name associated with the mortgage interest record.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of debt market ratios associated with the mortgage interest record.
    /// </summary>
    [JsonPropertyName("Ratios")]
    public List<DebtMarketRatio> DebtMarketRatios { get; set; } = [];
}
