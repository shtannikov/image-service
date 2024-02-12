namespace ImageService;

public class ImageMetadata
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Extension { get; set; }
    public long Size { get; set; }
    public DateOnly LastUpdateDate { get; set; }
}