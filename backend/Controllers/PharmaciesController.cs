using i_am_building_a_simple_graduation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Route("api/pharmacies")]
public class PharmaciesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        var query = db.Pharmacies.AsNoTracking().Where(x => x.WhatsAppPhone != "PENDING");
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Address.Contains(search));
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var pharmacy = await db.Pharmacies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.WhatsAppPhone != "PENDING");
        return pharmacy is null ? NotFound() : Ok(pharmacy);
    }
}
