using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record FitnessDto(
    DateTime Date,
    [Required] string ActivityName,
    int DurationMinutes,
    int CaloriesBurned,
    string? Notes
);

