using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/chat")]
public class ChatController(AppDbContext db) : ControllerBase
{
    [HttpGet("{doctorId:int}")]
    public async Task<IActionResult> Get(int doctorId) =>
        Ok(
            await db
                .ChatMessages.Where(x => x.UserId == CurrentUser.Id(User) && x.DoctorId == doctorId)
                .OrderBy(x => x.SentAt)
                .Select(x => new
                {
                    x.Id,
                    x.Message,
                    x.SentAt,
                    x.IsFromDoctor,
                    x.DoctorId,
                })
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(ChatDto dto)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == dto.DoctorId))
            return BadRequest();
        var row = new ChatMessage
        {
            UserId = CurrentUser.Id(User),
            DoctorId = dto.DoctorId,
            Message = dto.Message,
            SentAt = DateTime.Now,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created(
            "",
            new
            {
                row.Id,
                row.Message,
                row.SentAt,
                row.IsFromDoctor,
                row.DoctorId,
            }
        );
    }
}

