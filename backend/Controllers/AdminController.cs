using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs.Admin;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize(Roles = "Admin"), Route("api/admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() => Ok(new
    {
        users = await db.Users.CountAsync(x => x.Role != "Admin"),
        doctors = await db.Doctors.CountAsync(),
        pharmacies = await db.Pharmacies.CountAsync(),
        exerciseVideos = await db.Exercises.CountAsync()
    });

    [HttpGet("doctors")]
    public async Task<IActionResult> Doctors() => Ok(await db.Doctors.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor(DoctorCatalogDto dto)
    {
        if (!ValidWhatsApp(dto.WhatsAppPhone)) return BadRequest(new { message = "Enter a valid WhatsApp number with country code, for example +20 100 123 4567." });
        var doctor = new Doctor
        {
            Name = dto.Name.Trim(), Specialization = dto.Specialization.Trim(),
            Phone = Clean(dto.Phone), WhatsAppPhone = NormalizePhone(dto.WhatsAppPhone),
            Email = Clean(dto.Email), Description = Clean(dto.Description)
        };
        db.Doctors.Add(doctor);
        await db.SaveChangesAsync();
        return Created($"/api/doctors/{doctor.Id}", doctor);
    }

    [HttpPut("doctors/{id:int}")]
    public async Task<IActionResult> UpdateDoctor(int id, DoctorCatalogDto dto)
    {
        if (!ValidWhatsApp(dto.WhatsAppPhone)) return BadRequest(new { message = "Enter a valid WhatsApp number with country code, for example +20 100 123 4567." });
        var doctor = await db.Doctors.FindAsync(id);
        if (doctor is null) return NotFound();
        doctor.Name = dto.Name.Trim(); doctor.Specialization = dto.Specialization.Trim();
        doctor.Phone = Clean(dto.Phone); doctor.WhatsAppPhone = NormalizePhone(dto.WhatsAppPhone);
        doctor.Email = Clean(dto.Email); doctor.Description = Clean(dto.Description);
        await db.SaveChangesAsync();
        return Ok(doctor);
    }

    [HttpDelete("doctors/{id:int}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await db.Doctors.FindAsync(id);
        if (doctor is null) return NotFound();
        if (await db.Appointments.AnyAsync(x => x.DoctorId == id)) return Conflict(new { message = "This doctor has saved visit reminders. Update or remove those reminders before deleting the doctor." });
        db.Doctors.Remove(doctor);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("pharmacies")]
    public async Task<IActionResult> Pharmacies() => Ok(await db.Pharmacies.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    [HttpPost("pharmacies")]
    public async Task<IActionResult> CreatePharmacy(PharmacyCatalogDto dto)
    {
        if (!ValidWhatsApp(dto.WhatsAppPhone)) return BadRequest(new { message = "Enter a valid WhatsApp number with country code, for example +20 100 123 4567." });
        var pharmacy = new Pharmacy
        {
            Name = dto.Name.Trim(), Address = dto.Address.Trim(), Phone = Clean(dto.Phone),
            WhatsAppPhone = NormalizePhone(dto.WhatsAppPhone), OpeningHours = Clean(dto.OpeningHours)
        };
        db.Pharmacies.Add(pharmacy);
        await db.SaveChangesAsync();
        return Created($"/api/pharmacies/{pharmacy.Id}", pharmacy);
    }

    [HttpPut("pharmacies/{id:int}")]
    public async Task<IActionResult> UpdatePharmacy(int id, PharmacyCatalogDto dto)
    {
        if (!ValidWhatsApp(dto.WhatsAppPhone)) return BadRequest(new { message = "Enter a valid WhatsApp number with country code, for example +20 100 123 4567." });
        var pharmacy = await db.Pharmacies.FindAsync(id);
        if (pharmacy is null) return NotFound();
        pharmacy.Name = dto.Name.Trim(); pharmacy.Address = dto.Address.Trim();
        pharmacy.Phone = Clean(dto.Phone); pharmacy.WhatsAppPhone = NormalizePhone(dto.WhatsAppPhone);
        pharmacy.OpeningHours = Clean(dto.OpeningHours);
        await db.SaveChangesAsync();
        return Ok(pharmacy);
    }

    [HttpDelete("pharmacies/{id:int}")]
    public async Task<IActionResult> DeletePharmacy(int id)
    {
        var pharmacy = await db.Pharmacies.FindAsync(id);
        if (pharmacy is null) return NotFound();
        db.Pharmacies.Remove(pharmacy);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("exercises")]
    public async Task<IActionResult> Exercises() => Ok(await db.Exercises.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    [HttpPost("exercises")]
    public async Task<IActionResult> CreateExercise(ExerciseCatalogDto dto)
    {
        if (!ValidYouTubeUrl(dto.YoutubeUrl)) return BadRequest(new { message = "Use a valid YouTube video link." });
        var exercise = new Exercise { Name = dto.Name.Trim(), Description = Clean(dto.Description), YoutubeUrl = dto.YoutubeUrl.Trim(), Category = Clean(dto.Category) };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync();
        return Created("", exercise);
    }

    [HttpPut("exercises/{id:int}")]
    public async Task<IActionResult> UpdateExercise(int id, ExerciseCatalogDto dto)
    {
        if (!ValidYouTubeUrl(dto.YoutubeUrl))
            return BadRequest(new { message = "Use a secure YouTube video link." });
        var exercise = await db.Exercises.FindAsync(id);
        if (exercise is null) return NotFound();
        exercise.Name = dto.Name.Trim(); exercise.Description = Clean(dto.Description);
        exercise.YoutubeUrl = dto.YoutubeUrl.Trim(); exercise.Category = Clean(dto.Category);
        await db.SaveChangesAsync();
        return Ok(exercise);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool ValidWhatsApp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length is >= 8 and <= 15 && digits[0] != '0';
    }
    private static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.StartsWith("00", StringComparison.Ordinal) ? digits[2..] : digits;
    }
    private static bool ValidYouTubeUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps) return false;
        string? id = uri.Host.ToLowerInvariant() switch
        {
            "youtu.be" => uri.AbsolutePath.Trim('/').Split('/')[0],
            "youtube.com" or "www.youtube.com" => uri.AbsolutePath.StartsWith("/shorts/", StringComparison.Ordinal)
                ? uri.AbsolutePath[8..].Split('/')[0]
                : Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("v", out var videoId) ? videoId.ToString() : null,
            _ => null
        };
        return id is not null && Regex.IsMatch(id, "^[A-Za-z0-9_-]{11}$");
    }
}
