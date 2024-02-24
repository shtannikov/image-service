using DataConsistencyFunction;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Core;

public class ImagesDbContext: DbContext
{
    private readonly string _connectionString;

    public ImagesDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(_connectionString);
    }

    public DbSet<ImageMetadata> Metadata { get; set; }
}