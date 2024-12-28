using Amazon;
using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Mortgage.Data;
using DivinitySoftworks.Functions.Mortgage.Repositories;
using System.Data;
using Xunit;

namespace DivinitySoftworks.Functions.Mortgage.Tests;
public sealed class MortgageTests {
    readonly IAmazonDynamoDB _amazonDynamoDB;


    readonly MortgageInterest mortgageInterest = new() {
        PK = "E798B0C6-5065-4804-ABD1-C8C4761CB745",
        BankId = Guid.Parse("E798B0C6-5065-4804-ABD1-C8C4761CB745"),
        Date = DateTime.UtcNow.Ticks,
        Name = "ING",
        DebtMarketRatios = {
            new DebtMarketRatio {
                Ratio = 55,
                Years = 1,
                Interest = 4.76m
            },
            new DebtMarketRatio {
                Ratio = 65,
                Years = 1,
                Interest = 4.78m
            },
            new DebtMarketRatio {
                Ratio = 70,
                Years = 1,
                Interest = 4.88m
            }
        }
    };


    public MortgageTests() {
        _amazonDynamoDB = new AmazonDynamoDBClient("AKIAZQ3DPZD5G4A5CW4O", "BviEGcimWUIRaVyT4lPgN/qnDpPok+Cw3VssJaCz", RegionEndpoint.EUWest3);
    }

    [Fact]
    public async Task Mortgage_UpdateLatestAsync_ShouldUpdateSuccessfully() {
        try {

            MortgageInterestRepository mortgageInterestRepository = new(_amazonDynamoDB);

            if (mortgageInterest is null) 
                throw new DataException("In invalid Mortgage Interest object was inserted in the database.");

            // Check if the inserted record is the latest, if so, skip.
            if (mortgageInterest.IsLatest) return;

            // Get the latest record.
            MortgageInterest? latestMortgageInterest = await mortgageInterestRepository.ReadLatestAsync(mortgageInterest.PK);

            // Check if the latest record should be updated.
            if (latestMortgageInterest is not null && latestMortgageInterest.Date >= mortgageInterest.Date)
                throw new DataException("In invalid Mortgage Interest object. It was not the latest");

            mortgageInterest.MarkAsLatest();

            // Attempt to create the new mortgage interest in the repository
            if (!await mortgageInterestRepository.CreateAsync(mortgageInterest))
                throw new DataException("Creating new latest mortgage interest debt ratio has failed!");

            // Attempt to create the new mortgage interest in the repository
            if (latestMortgageInterest is not null)
                await mortgageInterestRepository.DeleteAsync(latestMortgageInterest.PK, latestMortgageInterest.SK);
        }
        catch (Exception exception) {

            throw exception;
        }
    }
}
