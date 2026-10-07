namespace i_am_building_a_simple_graduation.DTOs;

public record MeasurementDto(
    DateTime Date,
    decimal? Weight,
    string? BloodPressure,
    decimal? BloodSugar,
    int? HeartRate
);

