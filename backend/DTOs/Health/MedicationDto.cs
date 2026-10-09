using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record MedicationDto(
    [Required] string Name,
    string? Dosage,
    string? Instructions,
    DateTime StartDate,
    DateTime? EndDate
);
