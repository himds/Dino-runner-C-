using Dino_runner.Models;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<ShopItem> ShopItems => Set<ShopItem>();
    public DbSet<UserItem> UserItems => Set<UserItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Coins)
            .HasDefaultValue(0);

        modelBuilder.Entity<Score>()
            .HasOne(s => s.User)
            .WithMany(u => u.Scores)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAchievements)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.Achievement)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserItem>()
            .HasOne(ui => ui.User)
            .WithMany(u => u.UserItems)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserItem>()
            .HasOne(ui => ui.ShopItem)
            .WithMany(s => s.UserItems)
            .HasForeignKey(ui => ui.ShopItemId)
            .OnDelete(DeleteBehavior.Cascade);

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>().HasData(
            new Achievement
            {
                Id = 1,
                Title = "1000 points",
                Description = "Atteindre 1000 points en une partie",
                Condition = "score>=1000"
            },
            new Achievement
            {
                Id = 2,
                Title = "10 parties",
                Description = "Terminer 10 parties",
                Condition = "games>=10"
            }
        );

        modelBuilder.Entity<ShopItem>().HasData(
            new ShopItem { Id = 1, Name = "Double saut", Description = "Permet le double saut", Price = 200 },
            new ShopItem { Id = 2, Name = "Ralentissement", Description = "Ralentit la vitesse des obstacles", Price = 150 },
            new ShopItem { Id = 3, Name = "Bouclier", Description = "Survit à une collision", Price = 250 },
            new ShopItem { Id = 4, Name = "Boost de score", Description = "Touche E : score doublé pendant 30 secondes", Price = 300 }
        );
    }
}
