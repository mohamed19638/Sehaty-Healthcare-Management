using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/nutrition")]
public class NutritionController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .NutritionRecords.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderByDescending(x => x.Date)
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(NutritionDto dto)
    {
        var row = new NutritionRecord
        {
            UserId = CurrentUser.Id(User),
            Date = dto.Date == default ? DateTime.Now : dto.Date,
            Calories = dto.Calories,
            Protein = dto.Protein,
            Carbohydrates = dto.Carbohydrates,
            Fat = dto.Fat,
            WaterIntake = dto.WaterIntake,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, NutritionDto dto)
    {
        var row = await db.NutritionRecords.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        row.Date = dto.Date;
        row.Calories = dto.Calories;
        row.Protein = dto.Protein;
        row.Carbohydrates = dto.Carbohydrates;
        row.Fat = dto.Fat;
        row.WaterIntake = dto.WaterIntake;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.NutritionRecords.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

