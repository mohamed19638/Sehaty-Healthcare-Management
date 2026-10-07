using System.Text;
using i_am_building_a_simple_graduation.Data;
using i_am_building_a_simple_graduation.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste your JWT token here.",
        }
    );
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        }
    );
builder.Services.AddAuthorization();

var app = builder.Build();

// Adopt the database created by the earlier demo build without deleting its records.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.CanConnect())
    {
        var initialMigration = db.Database.GetMigrations().FirstOrDefault();
        if (
            initialMigration is not null
            && !db.Database.GetAppliedMigrations().Any()
            && await db.Users.AnyAsync()
        )
        {
            const string productVersion = "8.0.20";
            db.Database.ExecuteSqlInterpolated(
                $"CREATE TABLE [__EFMigrationsHistory] ([MigrationId] nvarchar(150) NOT NULL, [ProductVersion] nvarchar(32) NOT NULL, CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])); INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({initialMigration}, {productVersion});"
            );
        }
    }
    await db.Database.MigrateAsync();

    if (!db.Users.Any())
    {
        var demo = new User
        {
            FullName = "Demo User",
            Email = "demo@sehaty.com",
            DateOfBirth = new DateTime(2000, 1, 1),
            Gender = "Other",
            Height = 170,
            Weight = 70,
        };
        demo.PasswordHash = new PasswordHasher<User>().HashPassword(demo, "Demo123!");
        db.Users.Add(demo);
        db.SaveChanges();
    }

    var demoUser = db.Users.First(x => x.Email == "demo@sehaty.com");
    if (!db.Doctors.Any())
    {
        db.Doctors.AddRange(
            new Doctor
            {
                Name = "Dr. Sara Ahmed",
                Specialization = "Cardiology",
                Phone = "01000000001",
                Email = "sara@sehaty.com",
                Description = "Heart and blood pressure specialist",
            },
            new Doctor
            {
                Name = "Dr. Omar Hassan",
                Specialization = "Internal Medicine",
                Phone = "01000000002",
                Email = "omar@sehaty.com",
                Description = "General health consultations",
            },
            new Doctor
            {
                Name = "Dr. Lina Samir",
                Specialization = "Nutrition",
                Phone = "01000000003",
                Email = "lina@sehaty.com",
                Description = "Healthy lifestyle and nutrition",
            }
        );
        db.SaveChanges();
    }
    if (db.Pharmacies.Count() < 5)
    {
        var existingNames = db.Pharmacies.Select(x => x.Name).ToList();
        var samplePharmacies = new[]
        {
            new Pharmacy
            {
                Name = "Sehaty Pharmacy",
                Address = "Downtown",
                Phone = "01000000011",
                OpeningHours = "24/7",
            },
            new Pharmacy
            {
                Name = "Care Plus",
                Address = "Nasr City",
                Phone = "01000000012",
                OpeningHours = "9 AM - 11 PM",
            },
            new Pharmacy
            {
                Name = "El Ezaby Pharmacy",
                Address = "Heliopolis",
                Phone = "01000000013",
                OpeningHours = "10 AM - 12 AM",
            },
            new Pharmacy
            {
                Name = "Health Corner",
                Address = "Maadi",
                Phone = "01000000014",
                OpeningHours = "8 AM - 10 PM",
            },
            new Pharmacy
            {
                Name = "Green Pharmacy",
                Address = "New Cairo",
                Phone = "01000000015",
                OpeningHours = "24/7",
            },
        };
        db.Pharmacies.AddRange(samplePharmacies.Where(x => !existingNames.Contains(x.Name)));
        db.SaveChanges();
    }

    if (db.HealthMeasurements.Count(x => x.UserId == demoUser.Id) < 7)
    {
        for (var day = 6; day >= 0; day--)
        {
            var sampleTime = DateTime.Now.Date.AddDays(-day).AddHours(8 + day % 4);
            if (!db.HealthMeasurements.Any(x => x.UserId == demoUser.Id && x.Date == sampleTime))
                db.HealthMeasurements.Add(
                    new HealthMeasurement
                    {
                        UserId = demoUser.Id,
                        Date = sampleTime,
                        Weight = 70 - day * 0.1m,
                        BloodPressure = day % 2 == 0 ? "120/80" : "118/78",
                        BloodSugar = 92 + day % 5,
                        HeartRate = 70 + day % 8,
                    }
                );
        }
        db.SaveChanges();
    }
    if (!db.Medications.Any(x => x.UserId == demoUser.Id))
        db.Medications.Add(
            new Medication
            {
                UserId = demoUser.Id,
                Name = "Vitamin D",
                Dosage = "1 tablet",
                Instructions = "Once daily after breakfast",
                StartDate = DateTime.Today,
            }
        );
    if (!db.NutritionRecords.Any(x => x.UserId == demoUser.Id))
    {
        db.NutritionRecords.AddRange(
            new NutritionRecord
            {
                UserId = demoUser.Id,
                Date = DateTime.Now.Date.AddHours(9),
                Calories = 420,
                Protein = 18,
                Carbohydrates = 54,
                Fat = 14,
                WaterIntake = 0.4m,
            },
            new NutritionRecord
            {
                UserId = demoUser.Id,
                Date = DateTime.Now.Date.AddHours(14),
                Calories = 680,
                Protein = 35,
                Carbohydrates = 72,
                Fat = 24,
                WaterIntake = 0.6m,
            }
        );
    }
    if (!db.FitnessActivities.Any(x => x.UserId == demoUser.Id))
        db.FitnessActivities.Add(
            new FitnessActivity
            {
                UserId = demoUser.Id,
                Date = DateTime.Now.Date.AddDays(-1).AddHours(18),
                ActivityName = "Walking",
                DurationMinutes = 35,
                CaloriesBurned = 165,
                Notes = "Evening walk",
            }
        );
    if (!db.MedicalRecords.Any(x => x.UserId == demoUser.Id))
        db.MedicalRecords.Add(
            new MedicalRecord
            {
                UserId = demoUser.Id,
                Condition = "Seasonal allergy",
                Description = "Mild seasonal symptoms",
                DiagnosisDate = DateTime.Today.AddMonths(-2),
                Notes = "Demo record",
            }
        );
    if (!db.LabResults.Any(x => x.UserId == demoUser.Id))
        db.LabResults.Add(
            new LabResult
            {
                UserId = demoUser.Id,
                TestName = "Vitamin D",
                Result = "32",
                Unit = "ng/mL",
                TestDate = DateTime.Today.AddDays(-14),
                Notes = "Demo result",
            }
        );
    if (!db.Appointments.Any(x => x.UserId == demoUser.Id))
    {
        var doctor = db.Doctors.OrderBy(x => x.Id).First();
        db.Appointments.Add(
            new Appointment
            {
                UserId = demoUser.Id,
                DoctorId = doctor.Id,
                AppointmentDate = DateTime.Now.Date.AddDays(3).AddHours(11),
                Status = "Scheduled",
                Notes = "Routine follow-up",
            }
        );
        db.ChatMessages.Add(
            new ChatMessage
            {
                UserId = demoUser.Id,
                DoctorId = doctor.Id,
                Message = "مرحبًا، يمكنني مساعدتك في متابعة القياسات.",
                IsFromDoctor = true,
                SentAt = DateTime.Now.AddDays(-1),
            }
        );
    }
    db.SaveChanges();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();

