using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

/// <summary>
/// 分数提交请求 DTO。
/// 游戏结束后前端将 userId、本局分数，以及本局拾取的金币数发送到后端。
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

    /// <summary>
    /// 本局拾取的金币数量（例如游戏过程中捡到的金币）。
    /// 该值会累加到用户账户金币中。
    /// </summary>
    [Range(0, int.MaxValue)]
    public int CoinsCollected { get; set; }
}
