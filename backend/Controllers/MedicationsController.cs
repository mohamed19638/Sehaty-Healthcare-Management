using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/medications")]
public class MedicationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .Medications.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderByDescending(x => x.StartDate)
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(MedicationDto dto)
    {
        var row = new Medication
        {
            UserId = CurrentUser.Id(User),
            Name = dto.Name,
            Dosage = dto.Dosage,
            Instructions = dto.Instructions,
            StartDate = dto.StartDate == default ? DateTime.Now : dto.StartDate,
            EndDate = dto.EndDate,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, MedicationDto dto)
    {
        var row = await db.Medications.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        row.Name = dto.Name;
        row.Dosage = dto.Dosage;
        row.Instructions = dto.Instructions;
        row.StartDate = dto.StartDate;
        row.EndDate = dto.EndDate;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.Medications.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
