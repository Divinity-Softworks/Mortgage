namespace DivinitySoftworks.Functions.Mortgage.Settings;

/// <summary>
/// Settings for the database.
/// </summary>
public sealed record DatabaseSettings {
    /// <summary>
    /// The key name for the database settings.
    /// </summary>
    public const string KeyName = "Database";
    /// <summary>
    /// Gets or sets the credentials for the database.
    /// </summary>
    public CredentialSettings Credentials { get; set; } = default!;
    /// <summary>
    /// Indicates whether the database settings have credentials.
    /// </summary>
    public bool HasCredentials {
        get {
            if (Credentials is null) return false;
            if (string.IsNullOrWhiteSpace(Credentials.Key) && string.IsNullOrWhiteSpace(Credentials.Key)) return false;
            return true;
        }
    }
}
