using System.ComponentModel.DataAnnotations;

namespace user_service.Models.DTOs;

public sealed class UserRegistrationDto
{
    [Required, StringLength(80, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(128)]
    public string Password { get; set; } = string.Empty;

    [StringLength(150)] public string FullName { get; set; } = string.Empty;
    [StringLength(50)] public string PhoneNumber { get; set; } = string.Empty;
    [StringLength(300)] public string Address { get; set; } = string.Empty;
    [StringLength(80)] public string RoleProject { get; set; } = string.Empty;
}

public sealed class UserLoginDto
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public sealed class UserUpdateDto
{
    [Required, StringLength(80, MinimumLength = 3)] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = string.Empty;
    [StringLength(128, MinimumLength = 8)] public string? Password { get; set; }
    [StringLength(150)] public string FullName { get; set; } = string.Empty;
    [StringLength(50)] public string PhoneNumber { get; set; } = string.Empty;
    [StringLength(300)] public string Address { get; set; } = string.Empty;
    [StringLength(80)] public string RoleProject { get; set; } = string.Empty;
}
