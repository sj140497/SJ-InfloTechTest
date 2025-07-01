using System.ComponentModel.DataAnnotations;

namespace UserManagement.Common.DTOs.Contracts;

public sealed record CreateUserDto(
    [Required, MaxLength(100)] string Forename,
    [Required, MaxLength(100)] string Surname,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required] DateTime DateOfBirth,
    bool IsActive
);
