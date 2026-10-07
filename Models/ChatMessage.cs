namespace i_am_building_a_simple_graduation.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DoctorId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsFromDoctor { get; set; }
    public User User { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}

