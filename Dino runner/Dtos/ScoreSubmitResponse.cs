namespace Dino_runner.Dtos;

/// <summary>
/// 分数提交响应 DTO。
/// 用于返回本次结算后的分数记录 ID、金币奖励、总金币和新解锁成就。
/// </summary>
public class ScoreSubmitResponse
{
    /// <summary>
    /// 新生成的分数记录 ID。
    /// </summary>
    public int ScoreId { get; set; }

    /// <summary>
    /// 本局新获得的金币数量。
    /// </summary>
    public int NewCoins { get; set; }

    /// <summary>
    /// 当前用户的总金币。
    /// </summary>
    public int TotalCoins { get; set; }

    /// <summary>
    /// 本次新解锁的成就 ID 列表。
    /// </summary>
    public List<int> UnlockedAchievementIds { get; set; } = [];
}
