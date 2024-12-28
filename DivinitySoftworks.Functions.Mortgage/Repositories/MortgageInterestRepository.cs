using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DivinitySoftworks.Functions.Mortgage.Data;
using System.Text.Json;

namespace DivinitySoftworks.Functions.Mortgage.Repositories;

/// <summary>
/// Interface for handling mortgage interest data operations in a repository.
/// </summary>
public interface IMortgageInterestRepository {
    /// <summary>
    /// Creates a new mortgage interest record in the repository.
    /// </summary>
    /// <param name="mortgageInterest">The mortgage interest object to be created.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating success.</returns>
    Task<bool> CreateAsync(MortgageInterest mortgageInterest);
    /// <summary>
    /// Reads a mortgage interest record from the repository based on the primary key (pk) and sort key (sk).
    /// </summary>
    /// <param name="pk">The primary key of the mortgage interest record.</param>
    /// <param name="sk">The sort key of the mortgage interest record.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the mortgage interest object, or null if not found.</returns>
    Task<MortgageInterest?> ReadAsync(string pk, long sk);
    /// <summary>
    /// Reads mortgage interest records from the database based on the specified primary key.
    /// </summary>
    /// <param name="pk">The primary key used to filter the mortgage interest records.</param>
    /// <returns>A task representing the asynchronous operation, with a collection of <see cref="MortgageInterest"/> records.</returns>
    Task<IEnumerable<MortgageInterest>> ReadAsync(string pk);
    /// <summary>
    /// Reads the latest mortgage interest record from the repository based on the primary key (pk).
    /// </summary>
    /// <param name="pk">The primary key of the mortgage interest record.</param>
    /// <param name="sk">The sort key of the mortgage interest record.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the mortgage interest object, or null if not found.</returns>
    Task<MortgageInterest?> ReadLatestAsync(string pk);
    /// <summary>
    /// Deletes a mortgage interest record from the repository based on the primary key (pk) and sort key (sk).
    /// </summary>
    /// <param name="pk">The primary key of the mortgage interest record to delete.</param>
    /// <param name="sk">The sort key of the mortgage interest record to delete.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating success.</returns>
    Task<bool> DeleteAsync(string pk, long sk);
    /// <summary>
    /// Updates the specified <paramref name="mortgageInterest"/> record in the data store.
    /// </summary>
    /// <param name="mortgageInterest">The mortgage interest record to be updated.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating success.</returns>
    Task<bool> UpdateAsync(MortgageInterest mortgageInterest);
}

/// <summary>
/// Implementation of the IMortgageInterestRepository interface for interacting with DynamoDB to manage mortgage interest data.
/// </summary>
/// <param name="amazonDynamoDB">The DynamoDB service client used for database operations.</param>
public sealed class MortgageInterestRepository(IAmazonDynamoDB amazonDynamoDB) : IMortgageInterestRepository {
    readonly string _tableName = "Mortgage.DeptMarketRatios";
    readonly IAmazonDynamoDB _amazonDynamoDB = amazonDynamoDB;

    /// <inheritdoc/>
    public Task<bool> CreateAsync(MortgageInterest mortgageInterest) {
        return _amazonDynamoDB.CreateItemAsync(_tableName, mortgageInterest);
    }

    /// <inheritdoc/>
    public Task<MortgageInterest?> ReadAsync(string pk, long sk) {
        return _amazonDynamoDB.GetItemAsync<MortgageInterest?>(_tableName, pk.ToUpper(), sk);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MortgageInterest>> ReadAsync(string pk) {
        List<MortgageInterest> mortgageInterests = await _amazonDynamoDB.QueryAsync<MortgageInterest>(_tableName
            , "PK = :PK"
            , new {
                PK = pk.ToUpper()
            });

        return mortgageInterests;
    }

    /// <inheritdoc/>
    public async Task<MortgageInterest?> ReadLatestAsync(string pk) {
        List<MortgageInterest> mortgageInterests = await _amazonDynamoDB.QueryAsync<MortgageInterest>(_tableName
            , "PK = :PK"
            , new { 
                PK = $"{pk.ToUpper()}.$LATEST" 
            });

        return mortgageInterests.FirstOrDefault();
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string pk, long sk) {
        return _amazonDynamoDB.DeleteItemAsync(_tableName, pk.ToUpper(), sk);
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(MortgageInterest mortgageInterest) {
        return _amazonDynamoDB.PutItemAsync(_tableName, mortgageInterest);
    }
}
