using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

public class PurchaseRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ShopItemId { get; set; }
}
