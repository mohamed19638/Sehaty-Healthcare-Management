namespace i_am_building_a_simple_graduation.DTOs;

public record NutritionDto(
    DateTime Date,
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    decimal WaterIntake
);

