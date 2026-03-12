using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

/// <summary>
/// 创建用户请求 DTO。
/// 主要供 UsersController 使用。
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// 用户名。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
}
