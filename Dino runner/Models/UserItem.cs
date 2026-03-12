namespace Dino_runner.Models;

/// <summary>
/// 用户购买道具记录实体。
/// 代表“某个用户购买了某个道具”。
/// </summary>
public class UserItem
{
    /// <summary>
    /// 用户道具记录主键 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户 ID。
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 用户导航属性。
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// 商店商品 ID。
    /// </summary>
    public int ShopItemId { get; set; }

    /// <summary>
    /// 商品导航属性。
    /// </summary>
    public ShopItem? ShopItem { get; set; }

    /// <summary>
    /// 购买时间。
    /// </summary>
    public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;
}
