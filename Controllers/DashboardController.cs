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
        var since = DateTime.Now.Date.AddDays(-6);
        var appointments = await db
            .Appointments.Where(x => x.UserId == userId && x.AppointmentDate >= DateTime.Now)
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
                    x.Doctor.Name,
                    x.Doctor.Specialization,
                    x.Doctor.Phone,
                },
            })
            .ToListAsync();
        return Ok(
            new
            {
                user = User.Identity?.Name,
                measurements = await db
                    .HealthMeasurements.Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.Date)
                    .Take(30)
                    .ToListAsync(),
                medications = await db
                    .Medications.Where(x => x.UserId == userId)
                    .OrderBy(x => x.StartDate)
                    .ToListAsync(),
                appointments,
                nutrition = await db
                    .NutritionRecords.Where(x => x.UserId == userId && x.Date >= since)
                    .OrderBy(x => x.Date)
                    .ToListAsync(),
                activity = await db
                    .FitnessActivities.Where(x => x.UserId == userId && x.Date >= since)
                    .OrderByDescending(x => x.Date)
                    .Take(5)
                    .ToListAsync(),
            }
        );
    }
}

