using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Models;

public class Score
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]
    public int Value { get; set; }

    public DateTime Time { get; set; } = DateTime.UtcNow;
}
