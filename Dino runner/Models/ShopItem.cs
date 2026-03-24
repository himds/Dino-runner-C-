using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

public class ShopItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public int Price { get; set; }

    public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();
}
