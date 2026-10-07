using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/healthmeasurements")]
public class HealthMeasurementsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .HealthMeasurements.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderByDescending(x => x.Date)
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(MeasurementDto dto)
    {
        if (dto.Weight <= 0 || dto.BloodSugar < 0 || dto.HeartRate < 0)
            return BadRequest(new { message = "أدخل قيماً موجبة." });
        var row = new HealthMeasurement
        {
            UserId = CurrentUser.Id(User),
            Date = dto.Date == default ? DateTime.Now : dto.Date,
            Weight = dto.Weight,
            BloodPressure = dto.BloodPressure,
            BloodSugar = dto.BloodSugar,
            HeartRate = dto.HeartRate,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, MeasurementDto dto)
    {
        var row = await db.HealthMeasurements.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        row.Date = dto.Date;
        row.Weight = dto.Weight;
        row.BloodPressure = dto.BloodPressure;
        row.BloodSugar = dto.BloodSugar;
        row.HeartRate = dto.HeartRate;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.HealthMeasurements.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

