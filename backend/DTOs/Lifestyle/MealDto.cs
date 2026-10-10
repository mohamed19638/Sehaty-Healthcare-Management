using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record MealDto(
    [Required, StringLength(120)] string Name,
    [StringLength(500)] string? Description,
    [Range(0, 5000)] int? Calories
);
