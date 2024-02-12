using Microsoft.EntityFrameworkCore;

namespace ImageService;

public class ImagesDbContext: DbContext
{
    private readonly IConfiguration _configuration;

    public ImagesDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_configuration.GetConnectionString("Default"));
    }

   public DbSet<ImageMetadata> Metadata { get; set; }
}