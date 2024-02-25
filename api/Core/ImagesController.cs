using Amazon.S3;
using Amazon.S3.Model;
using ImageService.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageService.Core;

[Route("images")]
public class ImagesController : ControllerBase
{
    private readonly ImagesDbContext _imagesDbContext;
    private readonly IAmazonS3 _s3Client;
    private readonly IImageUploadedEventPublisher _imageUploadedEventPublisher;
    private readonly AwsConfiguration _awsConfig;

    public ImagesController(
        ImagesDbContext imagesDbContext,
        IAmazonS3 s3Client,
        IImageUploadedEventPublisher imageUploadedEventPublisher,
        IOptions<AwsConfiguration> awsConfig)
    {
        _imagesDbContext = imagesDbContext;
        _imageUploadedEventPublisher = imageUploadedEventPublisher;
        _s3Client = s3Client;
        _awsConfig = awsConfig.Value;
    }

    [HttpPost("content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageMetadata>> Upload(IFormFile image)
    {
        var imageName = Path.GetFileNameWithoutExtension(image.FileName);

        var existingImageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (existingImageMetadata is not null)
            return BadRequest($"An image with name '{imageName}' already exists");

        var newImageMetadata = new ImageMetadata
        {
            Name = imageName,
            Extension = Path.GetExtension(image.FileName),
            Size = image.Length,
            LastUpdateDate = DateOnly.FromDateTime(DateTime.Now)
        };

        var request = new PutObjectRequest
        {
            BucketName = _awsConfig.ImageS3Bucket,
            Key = newImageMetadata.Name,
            ContentType = image.ContentType,
            InputStream = image.OpenReadStream()
        };
        await _s3Client.PutObjectAsync(request);

        _imagesDbContext.Metadata.Add(newImageMetadata);
        await _imagesDbContext.SaveChangesAsync();

        await SendEventAsync(newImageMetadata);

        return Ok(newImageMetadata);
    }

    [HttpGet("content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download([FromQuery(Name="image-name")] string imageName)
    {
        var imageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (imageMetadata is null)
            return NotFound($"The image '{imageName}' is not found");

        var image = await _s3Client.GetObjectAsync(_awsConfig.ImageS3Bucket, imageName);
        return File(image.ResponseStream, image.Headers.ContentType);
    }

    [HttpDelete("content")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromQuery(Name="image-name")] string imageName)
    {
        var imageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (imageMetadata is null)
            return NotFound($"The image '{imageName}' is not found");

        await _s3Client.DeleteObjectAsync(_awsConfig.ImageS3Bucket, imageName);

        _imagesDbContext.Metadata.Remove(imageMetadata);
        await _imagesDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("metadata")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ImageMetadata> GetMetadata([FromQuery(Name="image-name")] string? imageName)
    {
        if (imageName is null)
        {
            var randomCount = new Random().Next(0, _imagesDbContext.Metadata.Count() - 1);
            var randomImageMetadata = _imagesDbContext.Metadata
                .Skip(randomCount)
                .FirstOrDefault();

            if (randomImageMetadata is null)
                return NotFound("There are no images yet");

            return Ok(randomImageMetadata);
        }

        var imageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (imageMetadata is null)
            return NotFound($"The image '{imageName}' is not found");

        return Ok(imageMetadata);
    }

    private async Task SendEventAsync(ImageMetadata newImage)
    {
        var downloadLink = Url.ActionLink(
            action: nameof(Download),
            values: new RouteValueDictionary { { "image-name", newImage.Name } });

        await _imageUploadedEventPublisher.SendAsync(
            new ImageUploadedEvent
            {
                NewImage = newImage,
                DownloadLink = downloadLink!
            });
    }
}