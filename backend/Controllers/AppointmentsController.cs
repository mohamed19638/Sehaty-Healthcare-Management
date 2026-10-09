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
    private static TimeZoneInfo CairoTimeZone
    {
        get
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); }
        }
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static bool IsValidSlot(DateTime utcDate)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDate, DateTimeKind.Utc), CairoTimeZone);
        return local.Minute % 30 == 0
            && local.Second == 0
            && local.Millisecond == 0
            && local.Hour >= 9
            && local.Hour < 17;
    }

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await db.Appointments
            .AsNoTracking()
            .Where(x => x.UserId == CurrentUser.Id(User))
            .OrderBy(x => x.AppointmentDate)
            .Select(x => new
            {
                x.Id,
                x.AppointmentDate,
                Status = x.AppointmentDate <= DateTime.UtcNow
                    ? "Past"
                    : x.Status == "Scheduled" ? "Requested" : x.Status,
                x.Notes,
                x.DoctorId,
                doctor = new
                {
                    name = x.Doctor.WhatsAppPhone == "PENDING" ? "Legacy sample entry" : x.Doctor.Name,
                    specialization = x.Doctor.WhatsAppPhone == "PENDING" ? "Legacy appointment" : x.Doctor.Specialization,
                    phone = x.Doctor.WhatsAppPhone == "PENDING" ? null : x.Doctor.Phone,
                    whatsAppPhone = x.Doctor.WhatsAppPhone == "PENDING" ? null : x.Doctor.WhatsAppPhone
                }
            })
            .ToListAsync());

    // The simple default schedule is 09:00-17:00 Cairo time in 30-minute slots.
    [HttpGet("availability")]
    public async Task<IActionResult> Availability([FromQuery] int doctorId, [FromQuery] DateOnly date)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == doctorId && x.WhatsAppPhone != "PENDING"))
            return NotFound(new { message = "Doctor not found." });

        var timeZone = CairoTimeZone;
        var slots = Enumerable.Range(0, 16)
            .Select(index => DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(9, 0)).AddMinutes(index * 30), DateTimeKind.Unspecified))
            .Select(local => TimeZoneInfo.ConvertTimeToUtc(local, timeZone))
            .Where(utc => utc > DateTime.UtcNow)
            .ToArray();
        var dayStart = slots.FirstOrDefault();
        var dayEnd = slots.LastOrDefault();
        var booked = slots.Length == 0
            ? new HashSet<DateTime>()
            : (await db.Appointments
                .Where(x => x.DoctorId == doctorId && x.AppointmentDate >= dayStart && x.AppointmentDate <= dayEnd)
                .Select(x => x.AppointmentDate)
                .ToListAsync())
                .ToHashSet();

        return Ok(slots
            .Where(slot => !booked.Contains(slot))
            .Select(slot => new { appointmentDate = DateTime.SpecifyKind(slot, DateTimeKind.Utc) }));
    }

    [HttpPost]
    public async Task<IActionResult> Post(AppointmentDto dto)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId && x.WhatsAppPhone != "PENDING"))
            return BadRequest(new { message = "Doctor not found." });

        var appointmentDate = ToUtc(dto.AppointmentDate);
        if (appointmentDate <= DateTime.UtcNow)
            return BadRequest(new { message = "Choose a future appointment time." });
        if (!IsValidSlot(appointmentDate))
            return BadRequest(new { message = "Choose an available 30-minute slot between 9:00 AM and 5:00 PM Cairo time." });

        var row = new Appointment
        {
            UserId = CurrentUser.Id(User),
            DoctorId = dto.DoctorId,
            AppointmentDate = appointmentDate,
            Notes = dto.Notes?.Trim(),
            Status = "Requested"
        };
        db.Appointments.Add(row);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "That time slot has just been booked. Please choose another." });
        }

        return Created("", new { row.Id, row.AppointmentDate, row.Status, row.Notes, row.DoctorId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, AppointmentDto dto)
    {
        var userId = CurrentUser.Id(User);
        var row = await db.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (row is null)
            return NotFound();
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId && x.WhatsAppPhone != "PENDING"))
            return BadRequest(new { message = "Doctor not found." });

        var appointmentDate = ToUtc(dto.AppointmentDate);
        if (appointmentDate <= DateTime.UtcNow || !IsValidSlot(appointmentDate))
            return BadRequest(new { message = "Choose a future 30-minute slot between 9:00 AM and 5:00 PM Cairo time." });
        var alreadyBooked = await db.Appointments.AnyAsync(x =>
            x.Id != id && x.DoctorId == dto.DoctorId && x.AppointmentDate == appointmentDate);
        if (alreadyBooked)
            return Conflict(new { message = "That time slot is already booked. Please choose another." });

        row.DoctorId = dto.DoctorId;
        row.AppointmentDate = appointmentDate;
        row.Notes = dto.Notes?.Trim();
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "That time slot has just been booked. Please choose another." }); }
        return Ok(new { row.Id, row.DoctorId, row.AppointmentDate, row.Status, row.Notes });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null)
            return NotFound();
        db.Appointments.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
