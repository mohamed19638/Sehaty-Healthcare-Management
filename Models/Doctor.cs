namespace i_am_building_a_simple_graduation.Models;

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

