using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

/// <summary>
/// 登录请求 DTO。
/// 前端登录时提交用户名和密码。
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码。
    /// </summary>
    [Required]
    [MinLength(4)]
    [MaxLength(50)]
    public string Password { get; set; } = string.Empty;
}
