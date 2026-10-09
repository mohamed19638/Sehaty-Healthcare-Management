using i_am_building_a_simple_graduation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Route("api/meals")]
public class MealsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var meals = await db.Meals.ToListAsync();
        return Ok(meals);
    }
}
