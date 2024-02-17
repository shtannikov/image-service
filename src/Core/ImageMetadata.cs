namespace ImageService.Core;

public class ImageMetadata
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Extension { get; init; }
    public long Size { get; init; }
    public DateOnly LastUpdateDate { get; init; }
}