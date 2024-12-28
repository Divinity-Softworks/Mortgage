using DivinitySoftworks.Functions.Mortgage.Data;
using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Contracts.Responses;

/// <summary>
/// Represents the response data for mortgage interest, including the bank ID, date, name, and associated debt market ratios.
/// </summary>
public sealed record MortgageInterestResponse {
    /// <summary>
    /// Initializes a new instance of the <see cref="MortgageInterestResponse"/> class.
    /// </summary>
    public MortgageInterestResponse() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MortgageInterestResponse"/> class from a <see cref="MortgageInterest"/> object.
    /// </summary>
    /// <param name="mortgageInterest">The mortgage interest data to initialize the response.</param>
    public MortgageInterestResponse(MortgageInterest mortgageInterest) {
        BankId = mortgageInterest.BankId;
        Date = mortgageInterest.Date;
        Name = mortgageInterest.Name;

        for (int i = 0; i < mortgageInterest.DebtMarketRatios.Count; i++) {
            DebtMarketRatios.Add(new DebtMarketRatioResponse {
                Ratio = mortgageInterest.DebtMarketRatios[i].Ratio,
                Years = mortgageInterest.DebtMarketRatios[i].Years,
                Interest = mortgageInterest.DebtMarketRatios[i].Interest
            });
        }
    }

    /// <summary>
    /// Gets or sets the bank identifier.
    /// </summary>
    [JsonPropertyName("bank_id")]
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the date of the mortgage interest record.
    /// </summary>
    [JsonPropertyName("date")]
    public long Date { get; init; } = default!;

    /// <summary>
    /// Gets or sets the name associated with the mortgage interest record.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of debt market ratios included in the mortgage interest response.
    /// </summary>
    [JsonPropertyName("ratios")]
    public List<DebtMarketRatioResponse> DebtMarketRatios { get; set; } = new List<DebtMarketRatioResponse>();
}
