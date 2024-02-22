using Microsoft.EntityFrameworkCore;

namespace ImageService.Core;

public class ImagesDbContext: DbContext
{
   public ImagesDbContext(DbContextOptions<ImagesDbContext> options) : base(options) {}

   public DbSet<ImageMetadata> Metadata { get; set; }
}