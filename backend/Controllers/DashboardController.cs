using i_am_building_a_simple_graduation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/dashboard")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = CurrentUser.Id(User);
        var dbUser = await db.Users.FindAsync(userId);
        decimal? bmi = null;
        if (dbUser != null && dbUser.Height > 0 && dbUser.Weight > 0)
        {
            var h = dbUser.Height.Value / 100m;
            bmi = Math.Round(dbUser.Weight.Value / (h * h), 2);
        }

        var since = DateTime.UtcNow.Date.AddDays(-6);
        var appointments = await db
            .Appointments.Where(x => x.UserId == userId && x.AppointmentDate >= DateTime.UtcNow)
            .OrderBy(x => x.AppointmentDate)
            .Take(4)
            .Select(x => new
            {
                x.Id,
                x.AppointmentDate,
                x.Status,
                x.Notes,
                x.DoctorId,
                doctor = new
                {
                    name = x.Doctor.WhatsAppPhone == "PENDING" ? "Legacy sample entry" : x.Doctor.Name,
                    specialization = x.Doctor.WhatsAppPhone == "PENDING" ? "Legacy appointment" : x.Doctor.Specialization,
                    phone = x.Doctor.WhatsAppPhone == "PENDING" ? null : x.Doctor.Phone,
                    whatsAppPhone = x.Doctor.WhatsAppPhone == "PENDING" ? null : x.Doctor.WhatsAppPhone,
                },
            })
            .ToListAsync();
        return Ok(
            new
            {
                user = new
                {
                    name = User.Identity?.Name,
                    height = dbUser?.Height,
                    weight = dbUser?.Weight,
                    bmi
                },
                measurements = await db
                    .HealthMeasurements.AsNoTracking().Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.Date)
                    .Take(30)
                    .ToListAsync(),
                medications = await db
                    .Medications.AsNoTracking().Where(x => x.UserId == userId)
                    .OrderBy(x => x.StartDate)
                    .ToListAsync(),
                appointments,
                activity = await db
                    .FitnessActivities.AsNoTracking().Where(x => x.UserId == userId && x.Date >= since)
                    .OrderByDescending(x => x.Date)
                    .Take(5)
                    .ToListAsync(),
            }
        );
    }
}
