namespace DivinitySoftworks.Functions.Mortgage.Settings;

/// <summary>
/// Settings for credentials.
/// </summary>
public sealed record CredentialSettings {
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public string Key { get; set; } = default!;
    /// <summary>
    /// Gets or sets the secret.
    /// </summary>
    public string Secret { get; set; } = default!;
}
