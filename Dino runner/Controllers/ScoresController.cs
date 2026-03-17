using Dino_runner.Data;
using Dino_runner.Dtos;
using Dino_runner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

/// <summary>
/// 分数控制器。
/// 负责上传分数、查询排行榜、查询用户历史分数，以及在分数上传后判定成就。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScoresController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// 提交一局游戏的分数。
    /// 处理流程：写入分数 → 累加本局拾取金币 → 判定是否解锁新成就 → 返回结算结果。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ScoreSubmitResponse>> SubmitScore([FromBody] ScoreSubmitRequest request)
    {
        var user = await context.Users
            .Include(u => u.UserAchievements)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null) return NotFound(new { message = "用户不存在" });

        var score = new Score
        {
            UserId = user.Id,
            Value = request.Value,
            Time = DateTime.UtcNow
        };

        context.Scores.Add(score);

        // 金币来源：由前端上报的“本局拾取金币数”（游戏里捡到的金币）。
        var coinsEarned = request.CoinsCollected;
        user.Coins += coinsEarned;

        await context.SaveChangesAsync();

        // 根据本局分数和累计数据判定是否解锁成就。
        var unlocked = await EvaluateAchievements(user.Id, request.Value);

        await context.SaveChangesAsync();

        return Ok(new ScoreSubmitResponse
        {
            ScoreId = score.Id,
            NewCoins = coinsEarned,
            TotalCoins = user.Coins,
            UnlockedAchievementIds = unlocked
        });
    }

    /// <summary>
    /// 获取排行榜。
    /// 默认返回前 20 条，支持通过 limit 控制返回数量。
    /// </summary>
    [HttpGet("top")]
    public async Task<ActionResult<IEnumerable<object>>> TopScores([FromQuery] int limit = 20)
    {
        var top = await context.Scores
            .Include(s => s.User)
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.Time)
            .Take(Math.Clamp(limit, 1, 100))
            .Select(s => new
            {
                s.Id,
                s.Value,
                s.Time,
                s.UserId,
                Username = s.User != null ? s.User.Username : string.Empty
            })
            .ToListAsync();

        return Ok(top);
    }

    /// <summary>
    /// 获取指定用户的历史分数记录。
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<Score>>> ScoresByUser(int userId)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == userId);
        if (!exists) return NotFound(new { message = "用户不存在" });

        var scores = await context.Scores
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Time)
            .ToListAsync();

        return Ok(scores);
    }

    /// <summary>
    /// 判定用户是否达成新的成就。
    /// 当前支持两类条件：
    /// 1. score>=X   单局分数达到阈值
    /// 2. games>=X   累计游戏局数达到阈值
    /// </summary>
    private async Task<List<int>> EvaluateAchievements(int userId, int currentScore)
    {
        var unlockedIds = new List<int>();

        var user = await context.Users
            .Include(u => u.Scores)
            .Include(u => u.UserAchievements)
            .FirstAsync(u => u.Id == userId);

        var unlockedSet = user.UserAchievements.Select(ua => ua.AchievementId).ToHashSet();
        var achievements = await context.Achievements.ToListAsync();

        foreach (var achievement in achievements)
        {
            if (unlockedSet.Contains(achievement.Id)) continue;

            var shouldUnlock = achievement.Condition switch
            {
                string c when c.StartsWith("score>=") => currentScore >= ParseThreshold(c),
                string c when c.StartsWith("games>=") => user.Scores.Count >= ParseThreshold(c),
                _ => false
            };

            if (shouldUnlock)
            {
                var record = new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    UnlockTime = DateTime.UtcNow
                };
                context.UserAchievements.Add(record);
                unlockedIds.Add(achievement.Id);
            }
        }

        return unlockedIds;
    }

    /// <summary>
    /// 从形如 score>=1000 或 games>=10 的条件字符串中提取阈值数字。
    /// 解析失败时返回极大值，避免误判成就。
    /// </summary>
    private static int ParseThreshold(string condition)
    {
        var parts = condition.Split(">=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 && int.TryParse(parts[1], out var value) ? value : int.MaxValue;
    }
}
