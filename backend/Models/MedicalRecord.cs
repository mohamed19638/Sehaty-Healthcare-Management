using System.Text.Json.Serialization;

namespace i_am_building_a_simple_graduation.Models;

public class MedicalRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public string? Notes { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
}
