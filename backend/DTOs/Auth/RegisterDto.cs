using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record RegisterDto(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    DateTime DateOfBirth,
    string? Gender,
    string? Phone,
    decimal? Height,
    decimal? Weight
);
