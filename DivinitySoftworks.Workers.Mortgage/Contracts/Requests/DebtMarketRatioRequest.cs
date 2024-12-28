using DivinitySoftworks.Workers.Mortgage.Data;

using System.Text.Json.Serialization;

namespace DivinitySoftworks.Workers.Mortgage.Contracts.Requests;

/// <summary>
/// Represents a request model for the Debt Market Ratio. It is used to encapsulate
/// the ratio, number of years, and interest rate in a structured format for transmission.
/// </summary>
public sealed record DebtMarketRatioRequest {
    /// <summary>
    /// Initializes a new instance of the <see cref="DebtMarketRatioRequest"/> class 
    /// by copying values from a <see cref="DebtMarketRatio"/> object.
    /// </summary>
    /// <param name="debtMarketRatio">The DebtMarketRatio object containing the ratio, years, and interest.</param>
    public DebtMarketRatioRequest(DebtMarketRatio debtMarketRatio) {
        Ratio = debtMarketRatio.Ratio;
        Years = debtMarketRatio.Years;
        Interest = debtMarketRatio.Interest;
    }

    /// <summary>
    /// Gets the debt-to-market ratio. 
    /// </summary>
    [JsonPropertyName("ratio")]
    public int Ratio { get; init; }

    /// <summary>
    /// Gets the number of years associated with the ratio.
    /// </summary>
    [JsonPropertyName("years")]
    public int Years { get; init; }

    /// <summary>
    /// Gets the interest rate applied to the ratio.
    /// </summary>
    [JsonPropertyName("interest")]
    public decimal Interest { get; init; }
}
