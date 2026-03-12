using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

/// <summary>
/// 用户实体。
/// 保存账号基础信息、金币、创建时间，以及与分数/成就/道具的关联。
/// </summary>
public class User
{
    /// <summary>
    /// 用户主键 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名，要求唯一。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希值。
    /// 项目中不会明文存储密码。
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 当前用户拥有的金币数量。
    /// </summary>
    public int Coins { get; set; }

    /// <summary>
    /// 用户创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 用户历史分数集合。
    /// </summary>
    public ICollection<Score> Scores { get; set; } = new List<Score>();

    /// <summary>
    /// 用户已解锁成就集合。
    /// </summary>
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    /// <summary>
    /// 用户已购买道具集合。
    /// </summary>
    public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();
}
