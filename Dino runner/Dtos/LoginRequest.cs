using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

public class LoginRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [MaxLength(50)]
    public string Password { get; set; } = string.Empty;
}

