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
    private static object ToResponse(FitnessActivity activity) => new
    {
        activity.Id,
        activity.Date,
        activity.ActivityName,
        activity.DurationMinutes,
        activity.Notes
    };

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var rows = await db.FitnessActivities.AsNoTracking()
            .Where(x => x.UserId == CurrentUser.Id(User))
            .OrderByDescending(x => x.Date)
            .ToListAsync();
        return Ok(rows.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Post(FitnessDto dto)
    {
        if (dto.DurationMinutes is < 1 or > 1440)
            return BadRequest(new { message = "Duration must be between 1 and 1,440 minutes." });

        var row = new FitnessActivity
        {
            UserId = CurrentUser.Id(User),
            Date = dto.Date == default ? DateTime.UtcNow : dto.Date.ToUniversalTime(),
            ActivityName = dto.ActivityName.Trim(),
            DurationMinutes = dto.DurationMinutes,
            Notes = dto.Notes?.Trim()
        };
        db.FitnessActivities.Add(row);
        await db.SaveChangesAsync();
        return Created("", ToResponse(row));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, FitnessDto dto)
    {
        if (dto.DurationMinutes is < 1 or > 1440)
            return BadRequest(new { message = "Duration must be between 1 and 1,440 minutes." });
        var row = await db.FitnessActivities.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null)
            return NotFound();

        row.Date = dto.Date == default ? row.Date : dto.Date.ToUniversalTime();
        row.ActivityName = dto.ActivityName.Trim();
        row.DurationMinutes = dto.DurationMinutes;
        row.Notes = dto.Notes?.Trim();
        await db.SaveChangesAsync();
        return Ok(ToResponse(row));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.FitnessActivities.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null)
            return NotFound();
        db.FitnessActivities.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
