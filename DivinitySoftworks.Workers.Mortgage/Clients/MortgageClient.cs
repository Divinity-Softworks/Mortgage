using DivinitySoftworks.Core.Web.Http;
using DivinitySoftworks.Core.Web.Http.Identity;
using DivinitySoftworks.Workers.Mortgage.Contracts.Requests;
using DivinitySoftworks.Workers.Mortgage.Contracts.Responses;
using DivinitySoftworks.Workers.Mortgage.Data;

namespace DivinitySoftworks.Workers.Mortgage.Clients;

/// <summary>
/// Interface for mortgage-related client operations.
/// </summary>
public interface IMortgageClient {
    /// <summary>
    /// Posts mortgage interest information and returns a response containing the mortgage interest data.
    /// </summary>
    /// <param name="mortgageInterest">The mortgage interest data to be posted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="MortgageInterestResponse"/> or null.</returns>
    Task<MortgageInterestResponse?> PostInterestsAsync(MortgageInterest mortgageInterest);
}

/// <summary>
/// Sealed class that implements the <see cref="IMortgageClient"/> interface,
/// using client credentials for authentication and authorization.
/// </summary>
public sealed class MortgageClient(HttpClient httpClient, TokenService tokenService)
    : ClientCredentialsClient(httpClient, tokenService), IMortgageClient {

    /// <summary>
    /// Posts mortgage interest information to the "mortgage" endpoint and returns the response.
    /// </summary>
    /// <param name="mortgageInterest">The mortgage interest data to be posted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="MortgageInterestResponse"/> or null.</returns>
    public Task<MortgageInterestResponse?> PostInterestsAsync(MortgageInterest mortgageInterest) {
        return PostAsync<MortgageInterestResponse?>("mortgage", new MortgageInterestRequest(mortgageInterest));
    }
}
