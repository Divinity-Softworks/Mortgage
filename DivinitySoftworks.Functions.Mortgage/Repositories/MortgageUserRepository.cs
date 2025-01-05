using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Mortgage.Data;

namespace DivinitySoftworks.Functions.Mortgage.Repositories;

/// <summary>
/// Repository interface for accessing mortgage interest user data in DynamoDB.
/// </summary>
public interface IMortgageUserRepository {
    /// <summary>
    /// Reads a single mortgage interest user by primary and sort keys.
    /// </summary>
    /// <param name="pk">The primary key (bank ID).</param>
    /// <param name="sk">The sort key (user ID).</param>
    /// <returns>A <see cref="MortgageInterestUser"/> instance or null if not found.</returns>
    Task<MortgageInterestUser?> ReadAsync(Guid pk, string sk);

    /// <summary>
    /// Reads all mortgage interest users with the specified primary key.
    /// </summary>
    /// <param name="pk">The primary key (bank ID).</param>
    /// <returns>A collection of <see cref="MortgageInterestUser"/> instances.</returns>
    Task<IEnumerable<MortgageInterestUser>> ReadAsync(Guid pk);
}

/// <summary>
/// Implementation of the <see cref="IMortgageUserRepository"/> interface, interacting with Amazon DynamoDB.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MortgageUserRepository"/> class.
/// </remarks>
/// <param name="amazonDynamoDB">An instance of <see cref="IAmazonDynamoDB"/>.</param>
public sealed class MortgageUserRepository(IAmazonDynamoDB amazonDynamoDB) : IMortgageUserRepository {
    private readonly string _tableName = "Mortgage.Users";
    private readonly IAmazonDynamoDB _amazonDynamoDB = amazonDynamoDB;

    /// <inheritdoc />
    public Task<MortgageInterestUser?> ReadAsync(Guid pk, string sk) {
        return _amazonDynamoDB.GetItemAsync<MortgageInterestUser?>(
            _tableName,
            pk.ToString().ToUpper(),
            sk
        );
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MortgageInterestUser>> ReadAsync(Guid pk) {
        var mortgageInterests = await _amazonDynamoDB.QueryAsync<MortgageInterestUser>(
            _tableName,
            "PK = :PK",
            new {
                PK = pk.ToString().ToUpper()
            }
        );

        return mortgageInterests;
    }
}
