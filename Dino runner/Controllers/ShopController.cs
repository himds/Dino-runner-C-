using Dino_runner.Data;
using Dino_runner.Dtos;
using Dino_runner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopController(AppDbContext context) : ControllerBase
{
    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<ShopItem>>> GetItems()
    {
        var items = await context.ShopItems.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserItems(int userId)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == userId);
        if (!exists) return NotFound(new { message = "用户不存在" });

        var items = await context.UserItems
            .Where(ui => ui.UserId == userId)
            .Select(ui => new
            {
                ui.Id,
                ui.ShopItemId,
                ui.PurchaseTime
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<object>> Purchase([FromBody] PurchaseRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null) return NotFound(new { message = "用户不存在" });

        var item = await context.ShopItems.FirstOrDefaultAsync(s => s.Id == request.ShopItemId);
        if (item == null) return NotFound(new { message = "商品不存在" });

        var owned = await context.UserItems.AnyAsync(ui => ui.UserId == user.Id && ui.ShopItemId == item.Id);
        if (owned) return Conflict(new { message = "已拥有该商品" });

        if (user.Coins < item.Price)
        {
            return BadRequest(new { message = "金币不足" });
        }

        user.Coins -= item.Price;
        context.UserItems.Add(new UserItem
        {
            UserId = user.Id,
            ShopItemId = item.Id,
            PurchaseTime = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        return Ok(new { message = "购买成功", user.Coins });
    }
}
