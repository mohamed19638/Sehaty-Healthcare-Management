using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record LoginDto([Required, EmailAddress] string Email, [Required] string Password);
