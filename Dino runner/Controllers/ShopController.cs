using Dino_runner.Data;
using Dino_runner.Dtos;
using Dino_runner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner.Controllers;

/// <summary>
/// 商城控制器。
/// 负责返回商品列表、查询用户已购道具，以及处理购买逻辑。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShopController(AppDbContext context) : ControllerBase
{
    /// <summary>
    /// 获取商城中的全部商品。
    /// 前端 shop.html 使用该接口渲染商品卡片。
    /// </summary>
    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<ShopItem>>> GetItems()
    {
        var items = await context.ShopItems.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// 获取指定用户已购买的道具列表。
    /// 前端会根据 shopItemId 判断该用户是否拥有某个功能型道具。
    /// </summary>
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

    /// <summary>
    /// 购买一个商城道具。
    /// 核心流程：校验用户 → 校验商品 → 检查是否已拥有 → 检查金币 → 扣费并写入购买记录。
    /// </summary>
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

        // 扣除金币，并记录本次购买。
        user.Coins -= item.Price;
        context.UserItems.Add(new UserItem
        {
            UserId = user.Id,
            ShopItemId = item.Id,
            PurchaseTime = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // 返回购买后的最新金币，方便前端立即刷新页面。
        return Ok(new { message = "购买成功", user.Coins });
    }
}
