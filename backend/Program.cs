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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configure Jwt:Key with a random secret of at least 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5500", "http://127.0.0.1:5500"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Apply schema changes and add only a demo login. No invented doctors, pharmacies,
// meals, videos, or health measurements are seeded.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.CanConnect())
    {
        var initialMigration = db.Database.GetMigrations().FirstOrDefault();
        if (initialMigration is not null && !db.Database.GetAppliedMigrations().Any())
        {
            const string productVersion = "8.0.20";
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @migrationId nvarchar(150) = {initialMigration};
                DECLARE @productVersion nvarchar(32) = {productVersion};
                DECLARE @adoptSql nvarchar(max) = N'
                    IF EXISTS (SELECT 1 FROM [dbo].[Users])
                    BEGIN
                        IF OBJECT_ID(N''dbo.__EFMigrationsHistory'', N''U'') IS NULL
                            CREATE TABLE [dbo].[__EFMigrationsHistory]
                            (
                                [MigrationId] nvarchar(150) NOT NULL,
                                [ProductVersion] nvarchar(32) NOT NULL,
                                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                            );
                        IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = @migrationId)
                            INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                            VALUES (@migrationId, @productVersion);
                    END';
                IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
                    EXEC sys.sp_executesql @adoptSql,
                        N'@migrationId nvarchar(150), @productVersion nvarchar(32)',
                        @migrationId, @productVersion;
                """);
        }
    }

    await db.Database.MigrateAsync();

    if (!await db.Users.AnyAsync(x => x.Email == "demo@sehaty.com"))
    {
        var demo = new User
        {
            FullName = "Demo User",
            Email = "demo@sehaty.com",
            DateOfBirth = new DateTime(2000, 1, 1)
        };
        demo.PasswordHash = new PasswordHasher<User>().HashPassword(demo, "Demo123!");
        db.Users.Add(demo);
        await db.SaveChangesAsync();
    }

    var sharedWorkoutVideos = new[]
    {
        ("CUjV6LCAlvs", "15 Minute Ultimate Beginner Home Workout"),
        ("Ki605EYP7_Q", "Shared Workout Video 2"),
        ("PwXUHMKamP8", "10 Minute Standing Cardio HIIT Workout"),
        ("IT94xC35u6k", "20 Minute Beginner Fat-Burning Workout"),
        ("nz9LcwYagqI", "5 Minute Indoor Walking Workout"),
        ("KaIeBaxzIqs", "Shared Workout Video 6"),
        ("q2NZyW5EP5A", "5 Minute Beginner Cardio Workout"),
        ("y3KwVF-aBNQ", "20 Minute Step-to-the-Beat HIIT Workout"),
        ("Z8xbbC6KCus", "Shared Workout Video 9")
    };

    foreach (var (videoId, name) in sharedWorkoutVideos)
    {
        var youtubeUrl = $"https://youtu.be/{videoId}";
        if (!await db.Exercises.AnyAsync(x => x.YoutubeUrl == youtubeUrl))
        {
            db.Exercises.Add(new Exercise
            {
                Name = name,
                Category = "Workout",
                YoutubeUrl = youtubeUrl
            });
        }
    }

    await db.SaveChangesAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
