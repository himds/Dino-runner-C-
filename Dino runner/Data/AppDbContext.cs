using Dino_runner.Models;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Data;

/// <summary>
/// EF Core 数据库上下文。
/// 负责声明数据表、配置实体关系，以及初始化项目的种子数据。
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // 核心业务表。
    public DbSet<User> Users => Set<User>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<ShopItem> ShopItems => Set<ShopItem>();
    public DbSet<UserItem> UserItems => Set<UserItem>();

    /// <summary>
    /// 配置实体关系、索引、默认值和种子数据。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 用户名唯一，避免重复注册。
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // 新用户默认金币为 0。
        modelBuilder.Entity<User>()
            .Property(u => u.Coins)
            .HasDefaultValue(0);

        // 用户与分数：一对多。
        modelBuilder.Entity<Score>()
            .HasOne(s => s.User)
            .WithMany(u => u.Scores)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 用户与用户成就：一对多。
        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAchievements)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 成就定义与用户成就记录：一对多。
        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.Achievement)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);

        // 用户与已购道具：一对多。
        modelBuilder.Entity<UserItem>()
            .HasOne(ui => ui.User)
            .WithMany(u => u.UserItems)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 商店道具定义与用户已购道具记录：一对多。
        modelBuilder.Entity<UserItem>()
            .HasOne(ui => ui.ShopItem)
            .WithMany(s => s.UserItems)
            .HasForeignKey(ui => ui.ShopItemId)
            .OnDelete(DeleteBehavior.Cascade);

        SeedData(modelBuilder);
    }

    /// <summary>
    /// 初始化项目启动时就需要存在的基础数据。
    /// 包括默认成就和默认商城道具。
    /// </summary>
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
