namespace DivinitySoftworks.Workers.Mortgage.Data;

/// <summary>
/// Represents a debt market ratio, including the ratio, number of years, and the interest rate.
/// </summary>
public sealed record DebtMarketRatio {
    /// <summary>
    /// Gets or sets the debt-to-market ratio.
    /// </summary>
    public int Ratio { get; set; }

    /// <summary>
    /// Gets or sets the number of years associated with the debt market ratio.
    /// </summary>
    public int Years { get; set; }

    /// <summary>
    /// Gets or sets the interest rate applied to the debt market ratio.
    /// </summary>
    public decimal Interest { get; set; }
}
