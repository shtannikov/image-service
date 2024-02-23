using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace NotificationDispatchFunction;

public class Function
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _snsTopicArn;

    public Function()
    {
        _snsClient = new AmazonSimpleNotificationServiceClient();

        var configurationRoot = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        _snsTopicArn = configurationRoot["ImageUploadedSnsTopicArn"]!;
    }

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext lambdaContext)
    {
        foreach(var queueMessage in sqsEvent.Records)
        {
            var notification = queueMessage.Body;

            await _snsClient.PublishAsync(_snsTopicArn, notification);

            lambdaContext.Logger.LogInformation(
                $"The following notification was dispatched:{Environment.NewLine}{notification}");
        }
    }
}