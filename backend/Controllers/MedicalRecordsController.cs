using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/medicalrecords")]
public class MedicalRecordsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await db
                .MedicalRecords.Where(x => x.UserId == CurrentUser.Id(User))
                .OrderByDescending(x => x.DiagnosisDate)
                .ToListAsync()
        );

    [HttpPost]
    public async Task<IActionResult> Post(MedicalRecordDto dto)
    {
        var row = new MedicalRecord
        {
            UserId = CurrentUser.Id(User),
            Condition = dto.Condition,
            Description = dto.Description,
            DiagnosisDate = dto.DiagnosisDate,
            Notes = dto.Notes,
        };
        db.Add(row);
        await db.SaveChangesAsync();
        return Created("", row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, MedicalRecordDto dto)
    {
        var row = await db.MedicalRecords.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        row.Condition = dto.Condition;
        row.Description = dto.Description;
        row.DiagnosisDate = dto.DiagnosisDate;
        row.Notes = dto.Notes;
        await db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.MedicalRecords.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User)
        );
        if (row is null)
            return NotFound();
        db.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
