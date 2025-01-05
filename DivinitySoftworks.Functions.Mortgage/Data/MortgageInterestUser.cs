using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Data;

/// <summary>
/// Represents a mortgage interest user, containing details such as bank ID, user ID, and personal information.
/// </summary>
public sealed record MortgageInterestUser {

    /// <summary>
    /// Gets the primary key (PK), derived from the BankId.
    /// This property is ignored during serialization.
    /// </summary>
    [JsonIgnore]
    public Guid PK => BankId;

    /// <summary>
    /// Gets the sort key (SK), derived from the UserId.
    /// This property is ignored during serialization.
    /// </summary>
    [JsonIgnore]
    public string SK => UserId;

    /// <summary>
    /// Gets or sets the bank identifier. This serves as the primary key (PK).
    /// </summary>
    [JsonPropertyName("PK")]
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier. This serves as the sort key (SK).
    /// </summary>
    [JsonPropertyName("SK")]
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [JsonPropertyName("Email")]
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [JsonPropertyName("FirstName")]
    public string? FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [JsonPropertyName("LastName")]
    public string? LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of mortgages associated with the entity.
    /// </summary>
    [JsonPropertyName("Mortgages")]
    public List<Mortgage> Mortgages { get; set; } = [];

    /// <summary>
    /// Gets the full name by combining the first and last name, handling cases where either or both are null or whitespace.
    /// </summary>
    /// <returns>
    /// A string containing the full name:
    /// <list type="bullet">
    /// <item>
    /// <description>Returns an empty string if both FirstName and LastName are null or whitespace.</description>
    /// </item>
    /// <item>
    /// <description>Returns only LastName if FirstName is null or whitespace.</description>
    /// </item>
    /// <item>
    /// <description>Returns only FirstName if LastName is null or whitespace.</description>
    /// </item>
    /// <item>
    /// <description>Returns "FirstName LastName" if both are non-empty.</description>
    /// </item>
    /// </list>
    /// </returns>
    public string FullName {
        get {
            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)) 
                return string.Empty;
            if (string.IsNullOrWhiteSpace(FirstName)) 
                return LastName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LastName)) 
                return FirstName ?? string.Empty;
            return $"{FirstName} {LastName}";
        }
    }
}
