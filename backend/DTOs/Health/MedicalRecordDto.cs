using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record MedicalRecordDto(
    [Required] string Condition,
    string? Description,
    DateTime DiagnosisDate,
    string? Notes
);
