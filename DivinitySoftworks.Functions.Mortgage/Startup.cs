using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using DivinitySoftworks.AWS.Core.Data.DynamoDB.Settings;
using DivinitySoftworks.Core.Web.Security;
using DivinitySoftworks.Functions.Mortgage.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DivinitySoftworks.Functions.Mortgage;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup {
    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services) {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true);

        IConfigurationRoot configuration = builder.Build();
        services.AddSingleton<IConfiguration>(configuration);

        DatabaseSettings? databaseSettings = configuration.GetSection(DatabaseSettings.KeyName).Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("Database settings are missing.");

        if (databaseSettings.HasCredentials)
            services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(databaseSettings.Credentials.Key, databaseSettings.Credentials.Secret, RegionEndpoint.EUWest3));
        else
            services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.EUWest3));

        services.AddSimpleNotificationService(configuration);

        services.AddSingleton<IMortgageInterestRepository, MortgageInterestRepository>();
        services.AddSingleton<IMortgageUserRepository, MortgageUserRepository>();

        services.Configure<AuthorizationOptions>(configuration.GetSection(AuthorizationOptions.Authorization));
        services.AddOpenIdConnect();
    }
}
