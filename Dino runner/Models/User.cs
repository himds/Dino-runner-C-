using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dino_runner.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    public int Coins { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();
}
