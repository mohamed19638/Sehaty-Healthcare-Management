using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs.Health;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/bmi")]
public class BmiController(AppDbContext db) : ControllerBase
{
    private static object CreateResult(decimal? heightCm, decimal? weightKg, DateTime dateOfBirth)
    {
        var age = DateOnly.FromDateTime(DateTime.UtcNow).Year - DateOnly.FromDateTime(dateOfBirth).Year;
        if (DateOnly.FromDateTime(dateOfBirth) > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-age))
            age--;

        decimal? bmi = null;
        string category;
        if (heightCm is not > 0 || weightKg is not > 0)
        {
            category = "Add a valid height and weight";
        }
        else
        {
            var heightMeters = heightCm.Value / 100m;
            var exactBmi = weightKg.Value / (heightMeters * heightMeters);
            bmi = Math.Round(exactBmi, 1);
            category = age < 20
                ? "Not interpreted: adult categories are for ages 20 and older"
                : exactBmi switch
                {
                    < 18.5m => "Underweight",
                    < 25m => "Healthy weight",
                    < 30m => "Overweight",
                    _ => "Obesity category"
                };
        }

        return new
        {
            Height = heightCm,
            Weight = weightKg,
            Bmi = bmi,
            Category = category,
            Disclaimer = "BMI is a screening measure, not a diagnosis. Adult categories do not apply during pregnancy and may need individual interpretation."
        };
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == CurrentUser.Id(User));
        return user is null
            ? NotFound()
            : Ok(CreateResult(user.Height, user.Weight, user.DateOfBirth));
    }

    [HttpPut]
    public async Task<IActionResult> Put(BmiUpdateDto dto)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == CurrentUser.Id(User));
        if (user is null)
            return NotFound();

        user.Height = dto.Height;
        user.Weight = dto.Weight;
        await db.SaveChangesAsync();
        return Ok(CreateResult(user.Height, user.Weight, user.DateOfBirth));
    }
}
