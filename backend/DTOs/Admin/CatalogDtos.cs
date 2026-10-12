using System.ComponentModel.DataAnnotations;

namespace i_am_building_a_simple_graduation.DTOs.Admin;

public record DoctorCatalogDto(
    [Required, StringLength(120)] string Name,
    [Required, StringLength(120)] string Specialization,
    [StringLength(30)] string? Phone,
    [StringLength(30)] string? WhatsAppPhone,
    [EmailAddress, StringLength(160)] string? Email,
    [StringLength(500)] string? Description);

public record PharmacyCatalogDto(
    [Required, StringLength(120)] string Name,
    [Required, StringLength(300)] string Address,
    [StringLength(30)] string? Phone,
    [StringLength(30)] string? WhatsAppPhone,
    [StringLength(120)] string? OpeningHours);

public record ExerciseCatalogDto(
    [Required, StringLength(160)] string Name,
    [StringLength(500)] string? Description,
    [Required, Url, StringLength(500)] string YoutubeUrl,
    [StringLength(80)] string? Category);
