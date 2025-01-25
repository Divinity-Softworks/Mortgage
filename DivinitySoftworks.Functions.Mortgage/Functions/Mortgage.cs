using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleNotificationService.Model;
using DivinitySoftworks.AWS.Core.Web.Functions;
using DivinitySoftworks.Core.Extentions;
using DivinitySoftworks.Core.Net.EventBus;
using DivinitySoftworks.Core.Net.Mail;
using DivinitySoftworks.Core.Web.Errors;
using DivinitySoftworks.Core.Web.Security;
using DivinitySoftworks.Functions.Mortgage.Contracts.Requests;
using DivinitySoftworks.Functions.Mortgage.Contracts.Responses;
using DivinitySoftworks.Functions.Mortgage.Data;
using DivinitySoftworks.Functions.Mortgage.Repositories;
using System.Data;
using System.Net.Mail;
using System.Text.Json;
using static Amazon.Lambda.Annotations.APIGateway.HttpResults;

namespace DS.Functions;

/// <summary>
/// The Mortgage class provides functionality to handle mortgage-related API requests.
/// </summary>
/// <param name="authorizeService">The authorization service to verify user permissions.</param>
public sealed class Mortgage([FromServices] IAuthorizeService authorizeService) : ExecutableFunction(authorizeService) {
    const string RootBase = "/mortgage";
    const string RootResourceName = "DSMortgage";
    const string TopicArn = "arn:aws:sns:eu-west-3:654654294266:sns-notification-email";
    const string SenderAddress = "Mortgage @ Divinity Softworks <mortgage@divinity-softworks.com>";
    const string Subject = "New mortgage rates available.";

    /// <summary>
    /// Handles DynamoDB stream events, specifically reacting to new INSERT operations.
    /// The method checks if the inserted record is the latest mortgage interest rate.
    /// If the record is not the latest, it marks the record as the latest and updates the database accordingly.
    /// The previous latest record is deleted if necessary.
    /// </summary>
    /// <param name="dynamoEvent">The event triggered by a change in the DynamoDB stream.</param>
    /// <param name="context">The Lambda execution context.</param>
    /// <param name="mortgageInterestRepository">The repository interface used to interact with mortgage interest data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(UpdateLatestAsync)}")]
    public async Task UpdateLatestAsync(DynamoDBEvent dynamoEvent, ILambdaContext context,
        [FromServices] IPublisher publisher,
        [FromServices] IMortgageInterestRepository mortgageInterestRepository,
        [FromServices] IMortgageUserRepository mortgageUserRepository) {
        try {
            foreach (DynamoDBEvent.DynamodbStreamRecord? record in dynamoEvent.Records) {
                if (!record.EventName.Equals("INSERT", StringComparison.CurrentCultureIgnoreCase)) continue;

                MortgageInterest? mortgageInterest = record.Dynamodb.NewImage.ToJson().DeserializeJson<MortgageInterest>();

                if (mortgageInterest is null) {
                    context.Logger.LogError("In invalid Mortgage Interest object was inserted in the database.");
                    continue;
                }

                // Check if the inserted record is the latest, if so, skip.
                if (mortgageInterest.IsLatest) return;

                // Get the latest record.
                MortgageInterest? latestMortgageInterest = await mortgageInterestRepository.ReadLatestAsync(mortgageInterest.PK);

                // Check if the latest record should be updated.
                if (latestMortgageInterest is not null && latestMortgageInterest.Date >= mortgageInterest.Date)
                    continue;

                mortgageInterest.MarkAsLatest();

                // Attempt to create the new latest mortgage interest in the repository
                if (!await mortgageInterestRepository.CreateAsync(mortgageInterest))
                    throw new DataException("Creating new latest mortgage interest debt ratio has failed!");

                // Delete the old latest mortgage interest in the repository
                if (latestMortgageInterest is not null)
                    await mortgageInterestRepository.DeleteAsync(latestMortgageInterest.PK, latestMortgageInterest.SK);

                // Notify the users about the updated rates.
                IEnumerable<MortgageInterestUser> mortgageInterestUsers = await mortgageUserRepository.ReadAsync(mortgageInterest.BankId);

                if (!mortgageInterestUsers.Any()) {
                    context.Logger.LogInformation("No users to notify about the updated mortgage rates.");
                    return;
                }

                context.Logger.LogInformation("Will notify {Number} user(s) about the updated mortgage rates.", mortgageInterestUsers.Count());

                foreach (MortgageInterestUser mortgageInterestUser in mortgageInterestUsers) {
                    string dataRows = string.Empty;

                    foreach (DivinitySoftworks.Functions.Mortgage.Data.Mortgage mortgage in mortgageInterestUser.Mortgages.Distinct()) {
                        DebtMarketRatio? oldDebtMarketRatio = latestMortgageInterest?.DebtMarketRatios.FirstOrDefault(d => d.Years == mortgage.Years && d.Ratio == mortgage.Ratio);
                        DebtMarketRatio? newDebtMarketRatio = mortgageInterest.DebtMarketRatios.FirstOrDefault(d => d.Years == mortgage.Years && d.Ratio == mortgage.Ratio);

                        string oldRate = oldDebtMarketRatio != null
                            ? oldDebtMarketRatio.Interest.ToString("F2") + "%"
                            : "N/A";

                        string newRate = newDebtMarketRatio != null
                            ? newDebtMarketRatio.Interest.ToString("F2") + "%"
                            : "N/A";

                        string rateDifference = (oldDebtMarketRatio != null && newDebtMarketRatio != null)
                            ? (newDebtMarketRatio.Interest - oldDebtMarketRatio.Interest).ToString("F2") + "%"
                            : "N/A";

                        dataRows += @$" <tr>
                                            <td>{mortgage.Years}</td>
                                            <td>{mortgage.Ratio}</td>
                                            <td class=""old-rate"">{oldRate}</td>
                                            <td class=""new-rate"">{newRate}</td>
                                            <td class=""rate-difference"">{rateDifference}</td>
                                        </tr>";
                    }

                    EmailTemplateMessage emailMessage = new(new(SenderAddress), "mortgage-new-rates") {
                        To = [new MailAddress(mortgageInterestUser.Email, mortgageInterestUser.FullName).ToString()],
                        Subject = Subject,
                        Parameters = { { "DATAROWS", dataRows } }
                    };

                    await publisher.PublishAsync<string, PublishResponse>(TopicArn, JsonSerializer.Serialize(emailMessage)!);
                }
            }
        }
        catch (Exception exception) {
            context.Logger.LogError(exception.Message);
        }
    }

    /// <summary>
    /// Handles HTTP POST requests to add mortgage interest data to the system.
    /// </summary>
    /// <param name="context">Lambda context that provides runtime information.</param>
    /// <param name="request">The incoming API Gateway HTTP request.</param>
    /// <param name="mortgageInterestRequest">The mortgage interest request payload from the client.</param>
    /// <param name="mortgageInterestRepository">The repository interface to interact with mortgage interest data storage.</param>
    /// <returns>A task representing the result of the operation, which can be a success or failure response.</returns>
    /// <exception cref="DataException">Thrown when the creation of a new mortgage interest entry fails.</exception>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(PostAsync)}")]
    [HttpApi(LambdaHttpMethod.Post, $"{RootBase}")]
    public async Task<IHttpResult> PostAsync(
        ILambdaContext context,
        APIGatewayHttpApiV2ProxyRequest request,
        [FromBody] MortgageInterestRequest mortgageInterestRequest,
        [FromServices] IMortgageInterestRepository mortgageInterestRepository) {

        return await ExecuteAsync(Authorize.Required, context, request, async () => {
            // Attempt to read an existing mortgage interest by BankId and Date
            MortgageInterest? mortgageInterest = await mortgageInterestRepository.ReadAsync(
                mortgageInterestRequest.BankId.ToString().ToUpper(),
                mortgageInterestRequest.Date);

            // If an existing mortgage interest is found, return Ok (200) response
            if (mortgageInterest is not null)
                return Ok(new MortgageInterestResponse(mortgageInterest));

            // Create a new mortgage interest object
            mortgageInterest = new MortgageInterest {
                PK = mortgageInterestRequest.BankId.ToString(),
                BankId = mortgageInterestRequest.BankId,
                Name = mortgageInterestRequest.Name,
                Date = mortgageInterestRequest.Date,
            };

            // Add debt market ratios to the mortgage interest
            for (int i = 0; i < mortgageInterestRequest.DebtMarketRatios.Count; i++) {
                mortgageInterest.DebtMarketRatios.Add(new DebtMarketRatio {
                    Ratio = mortgageInterestRequest.DebtMarketRatios[i].Ratio,
                    Years = mortgageInterestRequest.DebtMarketRatios[i].Years,
                    Interest = mortgageInterestRequest.DebtMarketRatios[i].Interest
                });
            }

            // Attempt to create the new mortgage interest in the repository
            if (!await mortgageInterestRepository.CreateAsync(mortgageInterest))
                throw new DataException("Creating new mortgage interest debt ratio has failed!");

            // Return a Created (201) response with the created mortgage interest data
            return Created(null, new MortgageInterestResponse(mortgageInterest));
        });
    }

    /// <summary>
    /// Retrieves mortgage interest records by bank identifier.
    /// </summary>
    /// <param name="context">The Lambda context providing runtime information.</param>
    /// <param name="request">The API Gateway HTTP request.</param>
    /// <param name="identifier">The identifier of the bank to retrieve interest rates for.</param>
    /// <param name="mortgageInterestRepository">The repository for accessing mortgage interest records.</param>
    /// <returns>
    /// An <see cref="IHttpResult"/> containing a collection of mortgage interest records if found, 
    /// or a corresponding error response if the identifier is invalid or no records are found.
    /// </returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetByIdAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}/{{identifier}}")]
    public async Task<IHttpResult> GetByIdAsync(
        ILambdaContext context,
        APIGatewayHttpApiV2ProxyRequest request,
        string identifier,
        [FromServices] IMortgageInterestRepository mortgageInterestRepository) {

        return await ExecuteAsync(Authorize.Required, context, request, async () => {
            if (!Guid.TryParse(identifier, out Guid bankIdentifier))
                return BadRequest(new ErrorResponse("bad_request", "The 'identifier' url path parameter is in an invalid GUID format."));

            IEnumerable<MortgageInterest> mortgageInterests = await mortgageInterestRepository.ReadAsync(bankIdentifier.ToString());

            if (!mortgageInterests.Any())
                return NotFound(new ErrorResponse("not_found", $"No mortgage interest rates are found for the bank with identifier: '{identifier}'."));

            return Ok(mortgageInterests.Select(mortgageInterest => new MortgageInterestResponse(mortgageInterest)));
        });
    }

    /// <summary>
    /// Retrieves the latest mortgage interest record by bank identifier.
    /// </summary>
    /// <param name="context">The Lambda context providing runtime information.</param>
    /// <param name="request">The API Gateway HTTP request.</param>
    /// <param name="identifier">The identifier of the bank to retrieve the latest interest rate for.</param>
    /// <param name="mortgageInterestRepository">The repository for accessing mortgage interest records.</param>
    /// <returns>
    /// An <see cref="IHttpResult"/> containing the latest mortgage interest record if found, 
    /// or a corresponding error response if the identifier is invalid or no record is found.
    /// </returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetLatestByIdAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}/{{identifier}}/latest")]
    public async Task<IHttpResult> GetLatestByIdAsync(
        ILambdaContext context,
        APIGatewayHttpApiV2ProxyRequest request,
        string identifier,
        [FromServices] IMortgageInterestRepository mortgageInterestRepository) {

        return await ExecuteAsync(Authorize.Required, context, request, async () => {
            if (!Guid.TryParse(identifier, out Guid bankIdentifier))
                return BadRequest(new ErrorResponse("bad_request", "The 'identifier' url path parameter is in an invalid GUID format."));

            MortgageInterest? mortgageInterest = await mortgageInterestRepository.ReadLatestAsync(bankIdentifier.ToString());

            if (mortgageInterest is null)
                return NotFound(new ErrorResponse("not_found", $"The latest mortgage interest rates are not found for the bank with identifier: '{identifier}'."));

            return Ok(new MortgageInterestResponse(mortgageInterest));
        });
    }

    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetMailTestAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}/mail")]
    public async Task<IHttpResult> GetMailTestAsync(
        ILambdaContext context,
        APIGatewayHttpApiV2ProxyRequest request,
        [FromServices] IPublisher publisher,
        [FromServices] IMortgageInterestRepository mortgageInterestRepository,
        [FromServices] IMortgageUserRepository mortgageUserRepository) {

        return await ExecuteAsync(Authorize.AllowAnonymous, context, request, async () => {

            try {
                foreach (MortgageInterestUser mortgageInterestUser in (await mortgageUserRepository.ReadAsync(Guid.Parse("E798B0C6-5065-4804-ABD1-C8C4761CB745")))) {

                    MortgageInterest newMortgageInterest = new() {
                        Date = 0,
                        Name = "ING",
                        PK = "E798B0C6-5065-4804-ABD1-C8C4761CB745",
                    };

                    newMortgageInterest.DebtMarketRatios.Add(new() {
                        Years = 10,
                        Interest = 4.65m,
                        Ratio = 100
                    });

                    // Get the latest record.
                    MortgageInterest? latestMortgageInterest = await mortgageInterestRepository.ReadLatestAsync("E798B0C6-5065-4804-ABD1-C8C4761CB745");

                    if (latestMortgageInterest is null) return Accepted();

                    string dataRows = string.Empty;

                    foreach (DivinitySoftworks.Functions.Mortgage.Data.Mortgage mortgage in mortgageInterestUser.Mortgages.Distinct()) {
                        DebtMarketRatio? oldDebtMarketRatio = latestMortgageInterest.DebtMarketRatios.FirstOrDefault(d => d.Years == mortgage.Years && d.Ratio == mortgage.Ratio);
                        DebtMarketRatio? newDebtMarketRatio = newMortgageInterest.DebtMarketRatios.FirstOrDefault(d => d.Years == mortgage.Years && d.Ratio == mortgage.Ratio);

                        string oldRate = oldDebtMarketRatio != null
                            ? oldDebtMarketRatio.Interest.ToString("F2") + "%"
                            : "N/A";

                        string newRate = newDebtMarketRatio != null
                            ? newDebtMarketRatio.Interest.ToString("F2") + "%"
                            : "N/A";

                        string rateDifference = (oldDebtMarketRatio != null && newDebtMarketRatio != null)
                            ? (newDebtMarketRatio.Interest - oldDebtMarketRatio.Interest).ToString("F2") + "%"
                            : "N/A";


                        dataRows += @$" <tr>
                                            <td>{mortgage.Years}</td>
                                            <td>{mortgage.Ratio}</td>
                                            <td class=""old-rate"">{oldRate}</td>
                                            <td class=""new-rate"">{newRate}</td>
                                            <td class=""rate-difference"">{rateDifference}</td>
                                        </tr>";
                    }

                    EmailTemplateMessage emailMessage = new(new(SenderAddress), "mortgage-new-rates") {
                        To = [new MailAddress(mortgageInterestUser.Email, mortgageInterestUser.FullName).ToString()],
                        Subject = Subject,
                        Parameters = { { "DATAROWS", dataRows } }
                    };

                    PublishResponse? response = await publisher.PublishAsync<string, PublishResponse>(TopicArn, JsonSerializer.Serialize(emailMessage)!);

                    Console.WriteLine($"Message sent to topic {TopicArn}. Message ID: {response!.MessageId}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to publish message to SNS: {ex.Message}");
            }

            return Accepted();
        });
    }
}