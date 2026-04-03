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
        if (!exists) return NotFound(new { message = "Utilisateur introuvable" });

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
        if (user == null) return NotFound(new { message = "Utilisateur introuvable" });

        var item = await context.ShopItems.FirstOrDefaultAsync(s => s.Id == request.ShopItemId);
        if (item == null) return NotFound(new { message = "Article introuvable" });

        var owned = await context.UserItems.AnyAsync(ui => ui.UserId == user.Id && ui.ShopItemId == item.Id);
        if (owned) return Conflict(new { message = "Vous possédez déjà cet article" });

        if (user.Coins < item.Price)
        {
            return BadRequest(new { message = "Pièces insuffisantes" });
        }

        user.Coins -= item.Price;
        context.UserItems.Add(new UserItem
        {
            UserId = user.Id,
            ShopItemId = item.Id,
            PurchaseTime = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // 返回购买后的最新金币，方便前端立即刷新页面。
        return Ok(new { message = "Achat réussi", user.Coins });
    }
}
