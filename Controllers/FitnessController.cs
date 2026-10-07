using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/fitness")]
public class FitnessController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .FitnessActivities.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderByDescending(x => x.Date)
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(FitnessDto dto)
    {
        var row = new FitnessActivity
        {
            UserId = CurrentUser.Id(User),
            Date = dto.Date == default ? DateTime.Now : dto.Date,
            ActivityName = dto.ActivityName,
            DurationMinutes = dto.DurationMinutes,
            CaloriesBurned = dto.CaloriesBurned,
            Notes = dto.Notes,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, FitnessDto dto)
    {
        var row = await db.FitnessActivities.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        row.Date = dto.Date;
        row.ActivityName = dto.ActivityName;
        row.DurationMinutes = dto.DurationMinutes;
        row.CaloriesBurned = dto.CaloriesBurned;
        row.Notes = dto.Notes;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.FitnessActivities.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

