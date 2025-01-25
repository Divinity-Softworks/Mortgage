using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DivinitySoftworks.AWS.Core.Web.Functions;
using DivinitySoftworks.AWS.Core.Web.Functions.Contracts;
using DivinitySoftworks.Core.Web.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using static Amazon.Lambda.Annotations.APIGateway.HttpResults;

namespace DS.Functions;

/// <summary>
/// HealthCheck class performs a health check for the service.
/// </summary>
public sealed class HealthCheck([FromServices] IAuthorizeService authorizeService) : ExecutableFunction(authorizeService) {
    const string RootBase = "/health";
    const string RootResourceName = "DSHealth";

    /// <summary>
    /// Lambda function that performs a health check.
    /// </summary>
    /// <param name="request">The API Gateway HTTP request.</param>
    /// <returns>The health status of the service.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetHealthAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}")]
    public async Task<IHttpResult> GetHealthAsync(ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request) {
        return await ExecuteAsync(Authorize.AllowAnonymous, context, request,
             async () => {
                 // Implement your health check logic here
                 HealthCheckResponse response = new() {
                     Status = await CheckHealthAsync()
                 };

                 if (response.Status == HealthStatus.Healthy)
                     return Ok(response);

                 return InternalServerError(response);
             });
    }

    /// <summary>
    /// Checks the health of the service asynchronously.
    /// </summary>
    /// <returns>The health status of the service.</returns>
    private Task<HealthStatus> CheckHealthAsync() {
        return Task.FromResult(HealthStatus.Healthy);
    }
}
