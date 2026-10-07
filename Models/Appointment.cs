namespace i_am_building_a_simple_graduation.Models;

public class Appointment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Scheduled";
    public User User { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}

