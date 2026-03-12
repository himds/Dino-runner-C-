using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

/// <summary>
/// 商城购买请求 DTO。
/// 包含购买者用户 ID 和目标商品 ID。
/// </summary>
public class PurchaseRequest
{
    /// <summary>
    /// 购买者用户 ID。
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 被购买的商品 ID。
    /// </summary>
    [Required]
    public int ShopItemId { get; set; }
}
