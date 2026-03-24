using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

public class CreateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
}
