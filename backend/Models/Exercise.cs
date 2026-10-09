namespace i_am_building_a_simple_graduation.Models;
public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? Category { get; set; }
}
