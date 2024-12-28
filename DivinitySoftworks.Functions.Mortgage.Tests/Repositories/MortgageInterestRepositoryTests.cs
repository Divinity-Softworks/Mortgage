using Amazon;
using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Mortgage.Data;
using DivinitySoftworks.Functions.Mortgage.Repositories;
using Xunit;

namespace DivinitySoftworks.Functions.Mortgage.Tests.Repositories;

public class MortgageInterestRepositoryTests {
    readonly IAmazonDynamoDB _amazonDynamoDB;

    readonly MortgageInterest mortgageInterest = new() {
        PK = "E798B0C6-5065-4804-ABD1-C8C4761CB745",
        BankId = Guid.Parse("E798B0C6-5065-4804-ABD1-C8C4761CB745"),
        Date = DateTime.UtcNow.Date.Ticks,
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

    public MortgageInterestRepositoryTests() {
        _amazonDynamoDB = new AmazonDynamoDBClient("AKIAZQ3DPZD5G4A5CW4O", "BviEGcimWUIRaVyT4lPgN/qnDpPok+Cw3VssJaCz", RegionEndpoint.EUWest3);
    }

    [Fact]
    public async void CreateAsync() {
        MortgageInterestRepository repository = new(_amazonDynamoDB);
        bool result = await repository.CreateAsync(mortgageInterest);

        Assert.True(result);
    }
}
