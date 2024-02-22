namespace ImageService.Events;

public interface IImageUploadedEventPublisher
{
    Task SendAsync(ImageUploadedEvent imageUploadedEvent);
}