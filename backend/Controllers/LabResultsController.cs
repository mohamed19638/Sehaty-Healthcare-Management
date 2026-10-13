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
                .Select(x => new
                {
                    x.Id,
                    x.TestName,
                    x.Result,
                    x.Unit,
                    x.TestDate,
                    x.Notes,
                    HasImage = x.ImageData != null
                })
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

    [HttpPost("{id:int}/image"), RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int id, IFormFile image)
    {
        var row = await db.LabResults.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null) return NotFound();
        if (image is null || image.Length is <= 0 or > 5 * 1024 * 1024)
            return BadRequest(new { message = "Choose an image up to 5 MB." });

        await using var stream = new MemoryStream();
        await image.CopyToAsync(stream);
        var bytes = stream.ToArray();
        var contentType = DetectImageType(bytes);
        if (contentType is null)
            return BadRequest(new { message = "Only valid JPG, PNG, or WEBP images are accepted." });

        row.ImageData = bytes;
        row.ImageContentType = contentType;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id)
    {
        var row = await db.LabResults.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User));
        return row?.ImageData is null || row.ImageContentType is null
            ? NotFound()
            : File(row.ImageData, row.ImageContentType);
    }

    private static string? DetectImageType(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return "image/png";
        if (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff)
            return "image/jpeg";
        if (bytes.Length >= 12
            && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
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
