using Amazon.SQS;
using Microsoft.Extensions.Options;

namespace ImageService.Events;

public class ImageUploadedEventPublisher : IImageUploadedEventPublisher
{
    private readonly IAmazonSQS _sqsClient;
    private readonly AwsConfiguration _awsConfig;

    public ImageUploadedEventPublisher(
        IAmazonSQS sqsClient,
        IOptions<AwsConfiguration> awsConfig)
    {
        _sqsClient = sqsClient;
        _awsConfig = awsConfig.Value;
    }

    public async Task SendAsync(ImageUploadedEvent imageUploadedEvent)
    {
        await _sqsClient.SendMessageAsync(
            _awsConfig.ImageUploadedQueueUrl,
            imageUploadedEvent.ToNotification());
    }
}