using ImageService.Core;

namespace ImageService.Events;

public class ImageUploadedEvent
{
    public ImageMetadata NewImage { get; init; }
    public string DownloadLink { get; init; }
    
    public string ToNotification()
    {
        var fullImageName = $"{NewImage.Name}{NewImage.Extension}";
        var imageSize = $"{NewImage.Size / 1024} KB";

        return "A new image has been uploaded: " +
               $"{fullImageName} ({imageSize}). " +
               $"Download link: {DownloadLink}";
    }
}