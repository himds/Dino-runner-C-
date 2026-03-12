namespace Dino_runner.Models;

/// <summary>
/// 用户成就记录实体。
/// 代表“某个用户在某个时间解锁了某个成就”。
/// </summary>
public class UserAchievement
{
    /// <summary>
    /// 用户成就记录主键 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户 ID。
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 用户导航属性。
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// 成就 ID。
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// 成就导航属性。
    /// </summary>
    public Achievement? Achievement { get; set; }

    /// <summary>
    /// 解锁时间。
    /// </summary>
    public DateTime UnlockTime { get; set; } = DateTime.UtcNow;
}
