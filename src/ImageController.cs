using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageService;

[Route("images")]
public class ImageController : ControllerBase
{
    private readonly ImagesDbContext _imagesDbContext;
    private readonly IAmazonS3 _s3Client;
    private readonly string _s3Bucket;

    public ImageController(
        ImagesDbContext imagesDbContext,
        IAmazonS3 s3Client,
        IConfiguration configuration)
    {
        _imagesDbContext = imagesDbContext;
        _s3Client = s3Client;
        _s3Bucket = configuration.GetValue<string>("S3Bucket")!;
    }

    [HttpPost("content")]
    public async Task<IActionResult> Upload(IFormFile image)
    {
        var imageName = Path.GetFileNameWithoutExtension(image.FileName);

        var existingImageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (existingImageMetadata is not null)
            return BadRequest($"The image with name '{imageName}' already exists");

        var imageMetadata = new ImageMetadata
        {
            Name = imageName,
            Extension = Path.GetExtension(image.FileName),
            Size = image.Length,
            LastUpdateDate = DateOnly.FromDateTime(DateTime.Now)
        };

        var request = new PutObjectRequest
        {
            BucketName = _s3Bucket,
            Key = imageMetadata.Name,
            ContentType = image.ContentType,
            InputStream = image.OpenReadStream()
        };
        await _s3Client.PutObjectAsync(request);

        _imagesDbContext.Metadata.Add(imageMetadata);
        await _imagesDbContext.SaveChangesAsync();

        return Ok($"Success! S3 path: {_s3Bucket}/{imageMetadata.Name}");
    }

    [HttpGet("content")]
    public async Task<IActionResult> Download(string imageName)
    {
        var imageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (imageMetadata is null)
            return NotFound($"The image '{imageName}' is not found");

        var image = await _s3Client.GetObjectAsync(_s3Bucket, imageName);
        return File(image.ResponseStream, image.Headers.ContentType);
    }
    
    [HttpDelete("content")]
    public async Task<IActionResult> Delete(string imageName)
    {
        var imageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (imageMetadata is null)
            return Ok();

        await _s3Client.DeleteObjectAsync(_s3Bucket, imageName);

        _imagesDbContext.Metadata.Remove(imageMetadata);
        await _imagesDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("metadata")]
    public IActionResult GetMetadata(string? imageName)
    {
        if (imageName is null)
        {
            var randomCount = new Random().Next(0, _imagesDbContext.Metadata.Count() - 1);
            var randomImageMetadata = _imagesDbContext.Metadata
                .Skip(randomCount)
                .FirstOrDefault();

            IActionResult result = randomImageMetadata is null
                ? NotFound("There are no images yet")
                : Ok(randomImageMetadata);
            return result;
        }

        var imageMetadata = _imagesDbContext.Metadata.SingleOrDefault(m => m.Name == imageName);
        if (imageMetadata is null)
            return NotFound($"The image '{imageName}' is not found");

        return Ok(imageMetadata);
    }
}