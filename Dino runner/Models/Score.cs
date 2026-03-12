using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

/// <summary>
/// 分数实体。
/// 每完成一局游戏，系统都会生成一条分数记录。
/// </summary>
public class Score
{
    /// <summary>
    /// 分数记录主键 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 所属用户 ID。
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 分数记录所属用户导航属性。
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// 本局分数值。
    /// </summary>
    [Required]
    public int Value { get; set; }

    /// <summary>
    /// 分数提交时间。
    /// </summary>
    public DateTime Time { get; set; } = DateTime.UtcNow;
}
