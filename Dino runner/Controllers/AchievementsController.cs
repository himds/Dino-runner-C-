using Dino_runner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

/// <summary>
/// Provides achievement definitions and user unlock records.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AchievementsController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Return all achievement definitions.
    /// Used by achievements.html to render the full achievement list.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var list = await context.Achievements
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.Condition
            })
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>
    /// Return unlocked achievements for a single user.
    /// Frontend merges this list with all achievement definitions.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByUser(int userId)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == userId);
        if (!exists) return NotFound(new { message = "Utilisateur introuvable" });

        var unlocked = await context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => new
            {
                ua.AchievementId,
                ua.UnlockTime
            })
            .ToListAsync();

        return Ok(unlocked);
    }
}
