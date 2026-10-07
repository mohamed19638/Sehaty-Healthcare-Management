namespace i_am_building_a_simple_graduation.Models;

public class FitnessActivity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public string? Notes { get; set; }
    public User User { get; set; } = null!;
}

