using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

/// <summary>
/// 商店道具定义实体。
/// 用于描述商城中的可购买商品。
/// </summary>
public class ShopItem
{
    /// <summary>
    /// 商品主键 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品名称。
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述。
    /// </summary>
    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// 商品价格（金币）。
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 拥有该商品的用户记录集合。
    /// </summary>
    public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();
}
