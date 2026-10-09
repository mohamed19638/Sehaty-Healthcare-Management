using System.Text.Json.Serialization;

namespace i_am_building_a_simple_graduation.Models;

public class Medication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
}
