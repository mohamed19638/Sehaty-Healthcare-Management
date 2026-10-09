using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/labresults")]
public class LabResultsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .LabResults.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderByDescending(x => x.TestDate)
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(LabResultDto dto)
    {
        var row = new LabResult
        {
            UserId = CurrentUser.Id(User),
            TestName = dto.TestName,
            Result = dto.Result,
            Unit = dto.Unit,
            TestDate = dto.TestDate,
            Notes = dto.Notes,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, LabResultDto dto)
    {
        var row = await db.LabResults.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        row.TestName = dto.TestName;
        row.Result = dto.Result;
        row.Unit = dto.Unit;
        row.TestDate = dto.TestDate;
        row.Notes = dto.Notes;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.LabResults.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
