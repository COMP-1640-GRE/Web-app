using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Faculty> Faculty { get; set; }
    public DbSet<Template> Template { get; set; }
    public DbSet<Notification> Notification { get; set; }
    public DbSet<User> User { get; set; }
    
}
