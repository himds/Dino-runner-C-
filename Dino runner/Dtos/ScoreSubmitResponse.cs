namespace Dino_runner.Dtos;

public class ScoreSubmitResponse
{
    public int ScoreId { get; set; }
    public int NewCoins { get; set; }
    public int TotalCoins { get; set; }
    public List<int> UnlockedAchievementIds { get; set; } = [];
}
