namespace DivinitySoftworks.Workers.Mortgage.Data;

/// <summary>
/// Represents mortgage interest data, including the bank's unique identifier, 
/// the date of the mortgage interest, the bank's name, and a list of associated debt market ratios.
/// </summary>
public sealed record MortgageInterest {
    /// <summary>
    /// Gets or sets the unique identifier of the bank.
    /// </summary>
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets the date of the mortgage interest in Unix time format.
    /// </summary>
    public long Date { get; init; } = default!;

    /// <summary>
    /// Gets or sets the name associated with the mortgage interest data.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of debt market ratios associated with this mortgage interest.
    /// </summary>
    public List<DebtMarketRatio> DebtMarketRatios { get; set; } = [];
}
