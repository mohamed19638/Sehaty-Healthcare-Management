namespace i_am_building_a_simple_graduation.Models;

using System.Text.Json.Serialization;

public class FitnessActivity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    // Retained in the database so older user data is not lost; it is no longer
    // accepted by the API or returned to clients.
    [JsonIgnore]
    public int? CaloriesBurned { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? Notes { get; set; }
    public User User { get; set; } = null!;
}
