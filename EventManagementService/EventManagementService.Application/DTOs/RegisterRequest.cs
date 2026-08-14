using System.ComponentModel.DataAnnotations;

namespace EventManagementService.Application.DTOs;

public class RegisterRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? Role { get; set; }
}