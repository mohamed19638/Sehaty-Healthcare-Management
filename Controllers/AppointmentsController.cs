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
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .Appointments.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderBy(x => x.AppointmentDate)
                .Select(x => new
                {
                    x.Id,
                    x.AppointmentDate,
                    x.Status,
                    x.Notes,
                    x.DoctorId,
                    doctor = new
                    {
                        x.Doctor.Name,
                        x.Doctor.Specialization,
                        x.Doctor.Phone,
                    },
                })
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(AppointmentDto dto)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId))
            return BadRequest(new { message = "الطبيب غير موجود." });
        var row = new Appointment
        {
            UserId = CurrentUser.Id(User),
            DoctorId = dto.DoctorId,
            AppointmentDate = dto.AppointmentDate,
            Notes = dto.Notes,
            Status = "Scheduled",
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, AppointmentDto dto)
    {
        var row = await db.Appointments.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId))
            return BadRequest();
        row.DoctorId = dto.DoctorId;
        row.AppointmentDate = dto.AppointmentDate;
        row.Notes = dto.Notes;
        await db.SaveChangesAsync();
        return Ok(
            new
            {
                row.Id,
                row.DoctorId,
                row.AppointmentDate,
                row.Status,
                row.Notes,
            }
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.Appointments.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

