using System.Text.Json.Serialization;

namespace i_am_building_a_simple_graduation.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    [JsonIgnore]
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    [JsonIgnore]
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    [JsonIgnore]
    public ICollection<HealthMeasurement> HealthMeasurements { get; set; } =
        new List<HealthMeasurement>();
}
