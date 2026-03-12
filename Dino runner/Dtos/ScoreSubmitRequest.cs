using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

/// <summary>
/// 分数提交请求 DTO。
/// 游戏结束后前端将 userId 和本局分数发送到后端。
/// </summary>
public class ScoreSubmitRequest
{
    /// <summary>
    /// 提交分数的用户 ID。
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 本局分数。
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Value { get; set; }
}
