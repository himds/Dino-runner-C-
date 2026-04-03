using Dino_runner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AchievementsController(AppDbContext context) : ControllerBase
{
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
