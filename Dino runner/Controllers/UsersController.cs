using Dino_runner.Data;
using Dino_runner.Dtos;
using Dino_runner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

/// <summary>
/// Provides user creation and user profile queries.
/// Mostly useful for admin/debugging and aggregated player data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Create a user record.
    /// Note: normal project flow uses AuthController/Register instead.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        var exists = await context.Users.AnyAsync(u => u.Username == request.Username);
        if (exists)
        {
            return Conflict(new { message = "用户名已存在" });
        }

        var user = new User
        {
            Username = request.Username,
            CreateTime = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Return a single user with summary information:
    /// owned items, unlocked achievements, score count and best score.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await context.Users
            .Include(u => u.UserItems)
            .Include(u => u.UserAchievements)
            .Include(u => u.Scores)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Coins,
            user.CreateTime,
            items = user.UserItems.Select(i => i.ShopItemId),
            achievements = user.UserAchievements.Select(a => a.AchievementId),
            scoreCount = user.Scores.Count,
            bestScore = user.Scores.OrderByDescending(s => s.Value).FirstOrDefault()?.Value ?? 0
        });
    }

    /// <summary>
    /// Return a lightweight list of all users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> ListUsers()
    {
        var users = await context.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Coins,
                scoreCount = u.Scores.Count
            })
            .ToListAsync();

        return Ok(users);
    }
}
