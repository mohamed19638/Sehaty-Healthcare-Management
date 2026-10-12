using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/appointments")]
public class AppointmentsController(AppDbContext db) : ControllerBase
{
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
    };

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await db.Appointments
        .AsNoTracking()
        .Where(x => x.UserId == CurrentUser.Id(User))
        .OrderBy(x => x.AppointmentDate)
        .Select(x => new
        {
            x.Id,
            x.AppointmentDate,
            Status = "Personal reminder",
            x.Notes,
            x.DoctorId,
            doctor = new
            {
                name = x.Doctor.Name,
                specialization = x.Doctor.Specialization,
                phone = x.Doctor.Phone,
                whatsAppPhone = x.Doctor.WhatsAppPhone
            }
        }).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Post(AppointmentDto dto)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId))
            return BadRequest(new { message = "Doctor not found." });
        var date = ToUtc(dto.AppointmentDate);
        if (date <= DateTime.UtcNow)
            return BadRequest(new { message = "Choose a future date and time for your personal reminder." });
        var row = new Appointment
        {
            UserId = CurrentUser.Id(User),
            DoctorId = dto.DoctorId,
            AppointmentDate = date,
            Notes = dto.Notes?.Trim(),
            Status = "Personal reminder"
        };
        db.Appointments.Add(row);
        await db.SaveChangesAsync();
        return Created("", new { row.Id, row.AppointmentDate, row.Status, row.Notes, row.DoctorId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, AppointmentDto dto)
    {
        var row = await db.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null) return NotFound();
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId))
            return BadRequest(new { message = "Doctor not found." });
        var date = ToUtc(dto.AppointmentDate);
        if (date <= DateTime.UtcNow)
            return BadRequest(new { message = "Choose a future date and time for your personal reminder." });
        row.DoctorId = dto.DoctorId;
        row.AppointmentDate = date;
        row.Notes = dto.Notes?.Trim();
        row.Status = "Personal reminder";
        await db.SaveChangesAsync();
        return Ok(new { row.Id, row.DoctorId, row.AppointmentDate, row.Status, row.Notes });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null) return NotFound();
        db.Appointments.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
