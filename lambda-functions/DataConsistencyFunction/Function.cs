using Amazon;
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

    public async Task<string> FunctionHandler(LambdaEvent lambdaEvent, ILambdaContext context)
    {
        var eventSource = lambdaEvent.DetailType ?? "Unknown";
        context.Logger.LogInformation($"The function triggered by the following service: {eventSource}");

        var lostImages = await GetLostImages(context);

        return lostImages.Any()
            ? $"The following images are not found: {string.Join(", ", lostImages.Select(i => i.Name))}"
            : "Everything is consistent";
    }

    private async Task<IReadOnlyCollection<ImageMetadata>> GetLostImages(ILambdaContext context)
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
