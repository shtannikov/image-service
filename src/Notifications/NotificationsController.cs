using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageService.Notifications;

[Route("notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationsDbContext _notificationsDbContext;
    private readonly IAmazonSimpleNotificationService _snsService;
    private readonly AwsConfiguration _awsConfig;

    public NotificationsController(
        NotificationsDbContext notificationsDbContext,
        IAmazonSimpleNotificationService snsService,
        IOptions<AwsConfiguration> awsConfig)
    {
        _notificationsDbContext = notificationsDbContext;
        _snsService = snsService;
        _awsConfig = awsConfig.Value;
    }

    [HttpPost("subscriptions")]
    public async Task<IActionResult> Create(string email)
    {
        var existingSubscriber = _notificationsDbContext.Subscribers.SingleOrDefault(s => s.Email == email);
        if (existingSubscriber is not null)
            return BadRequest($"A subscriber with email '{email}' already exists");

        var snsRequest = new SubscribeRequest
        {
            TopicArn = _awsConfig.ImageUploadedSnsTopicArn,
            ReturnSubscriptionArn = true,
            Protocol = "email",
            Endpoint = email
        };
        try
        {
            var snsResponse = await _snsService.SubscribeAsync(snsRequest);

            var newSubscriber = new Subscriber
            {
                Email = email,
                SubscriptionArn = snsResponse.SubscriptionArn
            };
            _notificationsDbContext.Subscribers.Add(newSubscriber);
            await _notificationsDbContext.SaveChangesAsync();
        }
        catch (InvalidParameterException e)
        {
            return BadRequest(e.Message);
        }

        return Ok($"{email} is successfully subscribed");
    }

    [HttpDelete("subscriptions")]
    public async Task<IActionResult> Delete(string email)
    {
        var subscriber = _notificationsDbContext.Subscribers.SingleOrDefault(s => s.Email == email);
        if (subscriber is null)
            return NotFound($"The subscriber with email '{email}' is not found");

        try
        {
            await _snsService.UnsubscribeAsync(subscriber.SubscriptionArn);

            _notificationsDbContext.Subscribers.Remove(subscriber);
            await _notificationsDbContext.SaveChangesAsync();
        }
        catch (InvalidParameterException e)
        {
            return BadRequest(e.Message);
        }

        return Ok($"{email} is successfully unsubscribed");
    }
}