using Dino_runner.Data;
using Dino_runner.Dtos;
using Dino_runner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoresController(AppDbContext context) : ControllerBase
{
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

        var coinsEarned = request.Value / 10;
        user.Coins += coinsEarned;

        await context.SaveChangesAsync();

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

    private static int ParseThreshold(string condition)
    {
        var parts = condition.Split(">=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 && int.TryParse(parts[1], out var value) ? value : int.MaxValue;
    }
}
