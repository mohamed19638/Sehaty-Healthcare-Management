using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs;

public record LabResultDto(
    [Required] string TestName,
    [Required] string Result,
    string? Unit,
    DateTime TestDate,
    string? Notes
);

