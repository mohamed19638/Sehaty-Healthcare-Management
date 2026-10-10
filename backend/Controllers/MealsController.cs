using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/meals")]
public class MealsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = CurrentUser.Id(User);
        var meals = await db.Meals
            .AsNoTracking()
            .Where(x => x.UserId == null || x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.Calories,
                x.IsEstimate,
                x.ImageUrl,
                IsMine = x.UserId == userId
            })
            .ToListAsync();

        return Ok(meals);
    }

    [HttpPost]
    public async Task<IActionResult> Post(MealDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Enter a meal name." });

        var meal = new Meal
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Calories = dto.Calories,
            IsEstimate = dto.Calories.HasValue,
            UserId = CurrentUser.Id(User)
        };

        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        return Created("", new
        {
            meal.Id,
            meal.Name,
            meal.Description,
            meal.Calories,
            meal.IsEstimate,
            meal.ImageUrl,
            IsMine = true
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var meal = await db.Meals.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User));
        if (meal is null)
            return NotFound();

        db.Meals.Remove(meal);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
