using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

public class Achievement
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string Condition { get; set; } = string.Empty;

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
