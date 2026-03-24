using System.Security.Cryptography;
using System.Text;
using Dino_runner.Data;
using Dino_runner.Dtos;
using Dino_runner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext context) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
    {
        var exists = await context.Users.AnyAsync(u => u.Username == request.Username);
        if (exists)
        {
            return Conflict(new { message = "用户名已存在" });
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Username, request.Password),
            CreateTime = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Coins });
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        var hash = HashPassword(request.Username, request.Password);
        if (!string.Equals(user.PasswordHash, hash, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        return Ok(new { user.Id, user.Username, user.Coins });
    }

    private static string HashPassword(string username, string password)
    {
        using var sha = SHA256.Create();
        var raw = $"{username}:{password}";
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}

