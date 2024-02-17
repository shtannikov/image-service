using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace ImageService.Notifications;

public class NotificationDispatchService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly ILogger<NotificationDispatchService> _logger;
    private readonly AwsConfiguration _awsConfig;

    public NotificationDispatchService(
        IAmazonSQS sqsClient,
        IAmazonSimpleNotificationService snsClient,
        ILogger<NotificationDispatchService> logger,
        IOptions<AwsConfiguration> awsConfig)
    {
        _sqsClient = sqsClient;
        _snsClient = snsClient;
        _logger = logger;
        _awsConfig = awsConfig.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Dispatch();
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }

    private async Task Dispatch()
    {
        var receiveQueueMsgRequest = new ReceiveMessageRequest
        {
            QueueUrl = _awsConfig.ImageUploadedQueueUrl,
            
        };
        var receiveQueueMsgResponse = await _sqsClient.ReceiveMessageAsync(receiveQueueMsgRequest);
        foreach (var queueMessage in receiveQueueMsgResponse.Messages)
        {
            var notification = queueMessage.Body;

            await _snsClient.PublishAsync(_awsConfig.ImageUploadedSnsTopicArn, notification);
            await _sqsClient.DeleteMessageAsync(_awsConfig.ImageUploadedQueueUrl, queueMessage.ReceiptHandle);

            _logger.LogInformation(
                "A new notification was dispatched. The notification: {Notification}", notification);
        }
    }

}