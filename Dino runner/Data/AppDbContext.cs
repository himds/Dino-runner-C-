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
                Title = "Score 1000",
                Description = "累积分数达到 1000",
                Condition = "score>=1000"
            },
            new Achievement
            {
                Id = 2,
                Title = "Play 10 Games",
                Description = "完成 10 局游戏",
                Condition = "games>=10"
            }
        );

        modelBuilder.Entity<ShopItem>().HasData(
            new ShopItem { Id = 1, Name = "Double Jump", Description = "允许二段跳", Price = 200 },
            new ShopItem { Id = 2, Name = "Slow Speed", Description = "减慢障碍速度", Price = 150 },
            new ShopItem { Id = 3, Name = "Shield", Description = "一次碰撞免死", Price = 250 }
        );
    }
}
