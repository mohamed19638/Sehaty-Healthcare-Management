namespace i_am_building_a_simple_graduation.Models;

public class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? OpeningHours { get; set; }
}

