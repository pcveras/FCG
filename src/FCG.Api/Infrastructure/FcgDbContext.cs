using FCG.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Infrastructure;

public class FcgDbContext(DbContextOptions<FcgDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<UserGame> UserGames => Set<UserGame>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(254).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.HasMany(x => x.Library).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Game>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasMany(x => x.Promotions).WithOne().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Promotion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        });
        b.Entity<UserGame>(e =>
        {
            e.HasKey(x => new { x.UserId, x.GameId });
            e.Property(x => x.PricePaid).HasPrecision(18, 2);
            e.HasOne(x => x.Game).WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
