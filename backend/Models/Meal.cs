namespace i_am_building_a_simple_graduation.Models;
public class Meal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Calories { get; set; }
    public bool IsEstimate { get; set; }
    public string? ImageUrl { get; set; }
}
