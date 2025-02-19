namespace DivinitySoftworks.Functions.Mortgage.Data;

/// <summary>
/// Represents a mortgage with its ratio percentage and the number of years associated with it.
/// </summary>
public sealed record Mortgage {

    /// <summary>
    /// Gets or sets the mortgage ratio percentage, typically representing the loan-to-value ratio (LTV).
    /// </summary>
    public int Ratio { get; set; }

    /// <summary>
    /// Gets or sets the number of years associated with the mortgage, often used to calculate the repayment term.
    /// </summary>
    public int Years { get; set; }

    /// <summary>
    /// Gets or sets the date the mortgage has started.
    /// </summary>
    public long Date { get; init; } = default!;
}
