using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

/// <summary>
/// 成就定义实体。
/// 描述一个成就的名称、说明和解锁条件。
/// </summary>
public class Achievement
{
    /// <summary>
    /// 成就主键 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 成就标题。
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 成就描述文本。
    /// </summary>
    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// 成就条件表达式。
    /// 当前项目示例：score>=1000、games>=10。
    /// </summary>
    [MaxLength(100)]
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// 已解锁该成就的用户记录集合。
    /// </summary>
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
