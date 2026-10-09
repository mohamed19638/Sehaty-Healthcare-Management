using ClosedXML.Excel;
using i_am_building_a_simple_graduation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Controllers;

[ApiController, Authorize, Route("api/export")]
public class ExportController(AppDbContext db) : ControllerBase
{
    [HttpGet("measurements/xlsx")]
    public async Task<IActionResult> ExportMeasurementsXlsx()
    {
        var userId = CurrentUser.Id(User);
        var measurements = await db.HealthMeasurements
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Measurements");
        var headers = new[]
        {
            "Date and time (UTC)", "Weight (kg)", "Blood pressure (mmHg)", "Blood pressure status",
            "Blood sugar (mg/dL)", "Fasting test?", "Heart rate (bpm)", "Measured at rest?"
        };
        for (var column = 0; column < headers.Length; column++)
            sheet.Cell(1, column + 1).Value = headers[column];
        sheet.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

        for (var index = 0; index < measurements.Count; index++)
        {
            var item = measurements[index];
            var row = index + 2;
            sheet.Cell(row, 1).Value = DateTime.SpecifyKind(item.Date, DateTimeKind.Utc);
            sheet.Cell(row, 1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
            if (item.Weight.HasValue) sheet.Cell(row, 2).Value = (double)item.Weight.Value;
            sheet.Cell(row, 3).Value = item.BloodPressure ?? "";
            sheet.Cell(row, 4).Value = item.BloodPressure is null ? "Not recorded" : "See app for interpretation";
            if (item.BloodSugar.HasValue) sheet.Cell(row, 5).Value = (double)item.BloodSugar.Value;
            sheet.Cell(row, 6).Value = item.BloodSugarWasFasting switch { true => "Yes", false => "No", null => "Unknown" };
            if (item.HeartRate.HasValue) sheet.Cell(row, 7).Value = item.HeartRate.Value;
            sheet.Cell(row, 8).Value = item.HeartRateWasAtRest switch { true => "Yes", false => "No", null => "Unknown" };
        }

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        Response.Headers.CacheControl = "no-store";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "health-measurements.xlsx");
    }
}
