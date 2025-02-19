using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Contracts.Responses; 
/// <summary>
/// Represents a user's mortgage details, including the bank information and mortgage terms.
/// </summary>
internal sealed record UserMortgageResponse {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserMortgageResponse"/> class.
    /// </summary>
    /// <param name="bankId">The unique identifier of the bank associated with the mortgage.</param>
    /// <param name="mortgage">The mortgage data used to initialize the response.</param>
    public UserMortgageResponse(Guid bankId, Data.Mortgage mortgage) {
        BankId = bankId;
        Ratio = mortgage.Ratio;
        Years = mortgage.Years;
        Date = mortgage.Date;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the bank associated with the mortgage.
    /// </summary>
    [JsonPropertyName("bank_id")]
    public Guid BankId { get; set; }

    /// <summary>
    /// Gets or sets the mortgage ratio percentage, typically representing the loan-to-value ratio (LTV).
    /// </summary>
    [JsonPropertyName("ratio")]
    public int Ratio { get; set; }

    /// <summary>
    /// Gets or sets the number of years associated with the mortgage, often used to calculate the repayment term.
    /// </summary>
    [JsonPropertyName("years")]
    public int Years { get; set; }

    /// <summary>
    /// Gets or sets the date the mortgage has started.
    /// </summary>
    [JsonPropertyName("date")]
    public long Date { get; init; } = default!;
}
