using Microsoft.EntityFrameworkCore;

namespace ImageService.Notifications;

public class NotificationsDbContext: DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) {}

    public DbSet<Subscriber> Subscribers { get; set; }
}