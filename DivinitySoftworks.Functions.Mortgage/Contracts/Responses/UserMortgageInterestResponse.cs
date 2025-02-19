using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Mortgage.Contracts.Responses {
    /// <summary>
    /// Represents the response containing mortgage interest information for a specific user.
    /// </summary>
    internal sealed record UserMortgageInterestResponse {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the collection of mortgages associated with the user.
        /// </summary>
        [JsonPropertyName("mortgages")]
        public IEnumerable<UserMortgageResponse> Mortgages { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of mortgage interest rates associated with the user's mortgages.
        /// </summary>
        [JsonPropertyName("mortgage_interests")]
        public IEnumerable<MortgageInterestResponse> MortgageInterests { get; set; } = [];
    }
}
