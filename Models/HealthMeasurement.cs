namespace i_am_building_a_simple_graduation.Models;

public class HealthMeasurement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal? Weight { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? BloodSugar { get; set; }
    public int? HeartRate { get; set; }
    public User User { get; set; } = null!;
}

