using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

public class ScoreSubmitRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Value { get; set; }
}
