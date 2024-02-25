using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using ImageService.Core;
using Microsoft.Extensions.Configuration;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DataConsistencyFunction;

public class Function
{
    private readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    public async Task<APIGatewayProxyResponse> FunctionHandler(LambdaEvent lambdaEvent, ILambdaContext lambdaContext)
    {
        var eventSource = lambdaEvent.GetSource();
        lambdaContext.Logger.Log($"The function is triggered by the following service: {eventSource}");

        var lostImages = await GetLostImages();

        var result = lostImages.Any()
            ? new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"The following images are not found: {string.Join(", ", lostImages.Select(i => i.Name))}"
            }
            : new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Everything is consistent"
            };

        lambdaContext.Logger.Log($"Result: {JsonSerializer.Serialize(result)}");

        return result;
    }

    private async Task<IReadOnlyCollection<ImageMetadata>> GetLostImages()
    {
        var dbContext = new ImagesDbContext(_configuration["dbConnectionString"]!);

        var region = RegionEndpoint.GetBySystemName(_configuration["region"]!);
        var s3Client = new AmazonS3Client(region);

        var lostImages = new List<ImageMetadata>();
        foreach (var image in dbContext.Metadata)
        {
            var isLost = await IsImageLost(image, s3Client);
            if (isLost)
                lostImages.Add(image);
        }

        return lostImages;
    }

    private async Task<bool> IsImageLost(ImageMetadata image, IAmazonS3 s3Client)
    {
        var request = new ListObjectsV2Request {
            BucketName = _configuration["s3Bucket"]!,
            Prefix = image.Name,
            MaxKeys = 1
        };
        var response = await s3Client.ListObjectsV2Async(request);

        return response.S3Objects.Count == 0;
    }
}
