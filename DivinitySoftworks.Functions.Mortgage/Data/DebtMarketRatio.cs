namespace DivinitySoftworks.Functions.Mortgage.Data;

/// <summary>
/// Represents a debt market ratio, which includes the ratio, the number of years, and the interest rate.
/// </summary>
public sealed record DebtMarketRatio {
    /// <summary>
    /// Gets or sets the debt market ratio percentage.
    /// </summary>
    public int Ratio { get; set; }

    /// <summary>
    /// Gets or sets the number of years associated with the debt market ratio.
    /// </summary>
    public int Years { get; set; }

    /// <summary>
    /// Gets or sets the interest rate associated with the debt market ratio.
    /// </summary>
    public decimal Interest { get; set; }
}
