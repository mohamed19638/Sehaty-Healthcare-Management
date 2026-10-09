using System.Text.RegularExpressions;
using i_am_building_a_simple_graduation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Route("api/exercises")]
public class ExercisesController(AppDbContext db) : ControllerBase
{
    private static string? GetYouTubeVideoId(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return null;

        string? id = null;
        if (uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
            id = uri.AbsolutePath.Trim('/');
        else if (uri.Host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("m.youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            if (uri.AbsolutePath.Equals("/watch", StringComparison.OrdinalIgnoreCase))
                id = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("v", out var videoId)
                    ? videoId.ToString()
                    : null;
            else if (uri.AbsolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
                id = uri.AbsolutePath[8..].Split('/')[0];
        }

        return id is not null && Regex.IsMatch(id, "^[A-Za-z0-9_-]{11}$") ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var exercises = await db.Exercises.AsNoTracking().ToListAsync();
        return Ok(exercises.Select(x => new
        {
            x.Id,
            x.Name,
            x.Description,
            x.Category,
            VideoId = GetYouTubeVideoId(x.YoutubeUrl)
        }));
    }
}
