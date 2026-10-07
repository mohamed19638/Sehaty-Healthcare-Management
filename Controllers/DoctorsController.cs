using i_am_building_a_simple_graduation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Route("api/doctors")]
public class DoctorsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await db.Doctors.AsNoTracking().ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var doctor = await db.Doctors.FindAsync(id);
        return doctor is null ? NotFound() : Ok(doctor);
    }
}

