using System.ComponentModel.DataAnnotations;

namespace Dino_runner.Dtos;

/// <summary>
/// 注册请求 DTO。
/// 前端注册页面提交用户名和密码时使用。
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 用户名。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 注册密码。
    /// </summary>
    [Required]
    [MinLength(4)]
    [MaxLength(50)]
    public string Password { get; set; } = string.Empty;
}
