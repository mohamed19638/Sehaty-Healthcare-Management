using System.Globalization;
using System.Text.RegularExpressions;
using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.DTOs;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/healthmeasurements")]
public class HealthMeasurementsController(AppDbContext db) : ControllerBase
{
    private static int GetAge(DateTime dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var birthDate = DateOnly.FromDateTime(dateOfBirth);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
            age--;
        return age;
    }

    private static string BloodPressureStatus(string? value, int age)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Not recorded";
        if (age < 18)
            return "Adult reference categories do not apply";

        var match = Regex.Match(value.Trim(), "^(\\d{2,3})\\s*/\\s*(\\d{2,3})$");
        if (!match.Success
            || !int.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var systolic)
            || !int.TryParse(match.Groups[2].Value, CultureInfo.InvariantCulture, out var diastolic)
            || systolic < 50 || systolic > 300 || diastolic < 30 || diastolic > 200)
            return "Invalid format or value";

        if (systolic > 180 || diastolic > 120)
            return "Very high: repeat after one minute and contact a healthcare professional if it remains high";
        if (systolic < 90 || diastolic < 60)
            return "Below the usual adult range; interpretation depends on symptoms and medical history";
        if (systolic < 120 && diastolic < 80)
            return "Within the usual adult reference range";
        if (systolic < 130 && diastolic < 80)
            return "Elevated adult reading; repeat correctly and discuss persistent readings with a clinician";
        if (systolic < 140 && diastolic < 90)
            return "Above the usual adult reference range; repeat and discuss persistent readings with a clinician";
        return "Above the usual adult reference range; repeat and discuss with a clinician";
    }

    private static string BloodSugarStatus(decimal? value, bool? wasFasting, int age)
    {
        if (value is null)
            return "Not recorded";
        if (age < 18)
            return "Adult reference categories do not apply";
        if (wasFasting is not true)
            return "Not classified: fasting status is required for this reference";
        if (value < 0 || value > 1000)
            return "Invalid value";
        if (value < 54)
            return "Very low reading; seek prompt medical guidance, especially if you have symptoms";
        if (value < 70)
            return "Below the common adult reference threshold; consider symptoms and contact a clinician";
        if (value < 100)
            return "Within the usual fasting reference range";
        if (value < 126)
            return "Above the usual fasting reference range; discuss with a clinician";
        return "High fasting reading; this is not a diagnosis—contact a clinician for proper testing";
    }

    private static string HeartRateStatus(int? value, bool? wasAtRest, int age)
    {
        if (value is null)
            return "Not recorded";
        if (age < 18)
            return "Adult reference categories do not apply";
        if (wasAtRest is not true)
            return "Not classified: record a resting pulse for this reference";
        if (value < 0 || value > 300)
            return "Invalid value";
        if (value < 60 || value > 100)
            return "Outside the usual adult resting range; activity, medicines, symptoms, and history matter";
        return "Within the usual adult resting reference range";
    }

    private object WithStatuses(HealthMeasurement measurement, int age) => new
    {
        measurement.Id,
        measurement.UserId,
        measurement.Date,
        measurement.Weight,
        measurement.BloodPressure,
        BloodPressureStatus = BloodPressureStatus(measurement.BloodPressure, age),
        measurement.BloodSugar,
        measurement.BloodSugarWasFasting,
        BloodSugarStatus = BloodSugarStatus(measurement.BloodSugar, measurement.BloodSugarWasFasting, age),
        measurement.HeartRate,
        measurement.HeartRateWasAtRest,
        HeartRateStatus = HeartRateStatus(measurement.HeartRate, measurement.HeartRateWasAtRest, age)
    };

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = CurrentUser.Id(User);
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            return Unauthorized();

        var measurements = await db.HealthMeasurements
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        var age = GetAge(user.DateOfBirth);
        return Ok(measurements.Select(x => WithStatuses(x, age)));
    }

    [HttpPost]
    public async Task<IActionResult> Post(MeasurementDto dto)
    {
        if (dto.Weight is <= 0 or > 500
            || dto.BloodSugar is < 0 or > 1000
            || dto.HeartRate is < 0 or > 300)
            return BadRequest(new { message = "Enter valid positive measurement values." });

        if (!string.IsNullOrWhiteSpace(dto.BloodPressure)
            && !Regex.IsMatch(dto.BloodPressure.Trim(), "^\\d{2,3}\\s*/\\s*\\d{2,3}$"))
            return BadRequest(new { message = "Enter blood pressure as systolic/diastolic, for example 120/80." });

        var userId = CurrentUser.Id(User);
        var measuredAt = dto.Date == default ? DateTime.UtcNow : dto.Date.ToUniversalTime();
        if (measuredAt > DateTime.UtcNow.AddMinutes(5))
            return BadRequest(new { message = "A health measurement cannot be recorded in the future." });

        var row = new HealthMeasurement
        {
            UserId = userId,
            Date = measuredAt,
            Weight = dto.Weight,
            BloodPressure = dto.BloodPressure?.Trim(),
            BloodSugar = dto.BloodSugar,
            BloodSugarWasFasting = dto.BloodSugarWasFasting,
            HeartRate = dto.HeartRate,
            HeartRateWasAtRest = dto.HeartRateWasAtRest
        };
        db.HealthMeasurements.Add(row);
        if (dto.Weight.HasValue)
        {
            var latestWeightDate = await db.HealthMeasurements
                .Where(x => x.UserId == userId && x.Weight != null)
                .Select(x => (DateTime?)x.Date)
                .MaxAsync();
            if (latestWeightDate is null || measuredAt >= latestWeightDate.Value)
            {
                var user = await db.Users.SingleAsync(x => x.Id == userId);
                user.Weight = dto.Weight;
            }
        }
        await db.SaveChangesAsync();

        var dateOfBirth = await db.Users.Where(x => x.Id == userId).Select(x => x.DateOfBirth).SingleAsync();
        var age = GetAge(dateOfBirth);
        return Created("", WithStatuses(row, age));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, MeasurementDto dto)
    {
        var userId = CurrentUser.Id(User);
        var row = await db.HealthMeasurements.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (row is null)
            return NotFound();
        if (dto.Weight is <= 0 or > 500 || dto.BloodSugar is < 0 or > 1000 || dto.HeartRate is < 0 or > 300)
            return BadRequest(new { message = "Enter valid positive measurement values." });
        if (!string.IsNullOrWhiteSpace(dto.BloodPressure)
            && !Regex.IsMatch(dto.BloodPressure.Trim(), "^\\d{2,3}\\s*/\\s*\\d{2,3}$"))
            return BadRequest(new { message = "Enter blood pressure as systolic/diastolic, for example 120/80." });

        row.Date = dto.Date == default ? row.Date : dto.Date.ToUniversalTime();
        row.Weight = dto.Weight;
        row.BloodPressure = dto.BloodPressure?.Trim();
        row.BloodSugar = dto.BloodSugar;
        row.BloodSugarWasFasting = dto.BloodSugarWasFasting;
        row.HeartRate = dto.HeartRate;
        row.HeartRateWasAtRest = dto.HeartRateWasAtRest;
        await db.SaveChangesAsync();

        var dateOfBirth = await db.Users.Where(x => x.Id == userId).Select(x => x.DateOfBirth).SingleAsync();
        var age = GetAge(dateOfBirth);
        return Ok(WithStatuses(row, age));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await db.HealthMeasurements.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == CurrentUser.Id(User));
        if (row is null)
            return NotFound();
        db.HealthMeasurements.Remove(row);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
