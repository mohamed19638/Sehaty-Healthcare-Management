using System.Text.Json.Serialization;

namespace i_am_building_a_simple_graduation.Models;

public class LabResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public DateTime TestDate { get; set; }
    public string? Notes { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
}
