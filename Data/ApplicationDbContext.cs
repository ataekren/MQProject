using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
} 