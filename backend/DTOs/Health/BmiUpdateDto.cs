using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs.Health;

public record BmiUpdateDto(
    [Range(50, 300)] decimal Height,
    [Range(2, 500)] decimal Weight
);
