using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using ImageService;
using ImageService.Core;
using ImageService.Events;
using ImageService.Notifications;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AwsConfiguration>(
    builder.Configuration.GetSection("AWS"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDefaultAWSOptions(
        builder.Configuration.GetAWSOptions());
}

builder.Services.AddDbContext<ImagesDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddDbContext<NotificationsDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddAWSService<IAmazonSQS>();

builder.Services.AddTransient<IImageUploadedEventPublisher, ImageUploadedEventPublisher>();
builder.Services.AddHostedService<NotificationDispatchService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();