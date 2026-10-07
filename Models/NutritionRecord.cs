namespace i_am_building_a_simple_graduation.Models;

public class NutritionRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fat { get; set; }
    public decimal WaterIntake { get; set; }
    public User User { get; set; } = null!;
}

