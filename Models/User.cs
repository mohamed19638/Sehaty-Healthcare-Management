namespace i_am_building_a_simple_graduation.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public ICollection<HealthMeasurement> HealthMeasurements { get; set; } =
        new List<HealthMeasurement>();
}

