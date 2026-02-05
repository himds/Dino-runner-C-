namespace Dino_runner.Models;

public class UserItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int ShopItemId { get; set; }
    public ShopItem? ShopItem { get; set; }

    public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;
}
