using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageService.Health;

[Route("health")]
public class HealthController : ControllerBase
{
    private readonly IAmazonLambda _lambdaClient;
    private readonly AwsConfiguration _awsConfig;

    public HealthController(
        IAmazonLambda lambdaClient,
        IOptions<AwsConfiguration> awsConfig)
    {
        _lambdaClient = lambdaClient;
        _awsConfig = awsConfig.Value;
    }

    [HttpGet("consistency-probe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Check()
    {
        var response = await _lambdaClient.InvokeAsync(
            new InvokeRequest
            {
                FunctionName = _awsConfig.ConsistencyCheckFunctionName,
                Payload = "{ \"detail-type\": \"Image Service\" }"
            });
        var deserializedResponse = await JsonSerializer.DeserializeAsync<APIGatewayProxyResponse>(response.Payload);

        return StatusCode(deserializedResponse!.StatusCode, deserializedResponse.Body);
    }
}