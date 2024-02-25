namespace ImageService;

public class AwsConfiguration
{
    public string ImageS3Bucket { get; init; }
    public string ImageUploadedQueueUrl { get; init; }
    public string ImageUploadedSnsTopicArn { get; init; }
    public string HealthCheckFunctionName { get; init; }
}