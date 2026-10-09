using i_am_building_a_simple_graduation.Models;
using Microsoft.EntityFrameworkCore;

namespace i_am_building_a_simple_graduation.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<HealthMeasurement> HealthMeasurements => Set<HealthMeasurement>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<NutritionRecord> NutritionRecords => Set<NutritionRecord>();
    public DbSet<FitnessActivity> FitnessActivities => Set<FitnessActivity>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Exercise> Exercises => Set<Exercise>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<User>().Property(x => x.Height).HasPrecision(6, 2);
        b.Entity<User>().Property(x => x.Weight).HasPrecision(6, 2);
        b.Entity<HealthMeasurement>().Property(x => x.Weight).HasPrecision(6, 2);
        b.Entity<HealthMeasurement>().Property(x => x.BloodSugar).HasPrecision(6, 2);
        b.Entity<NutritionRecord>().Property(x => x.Protein).HasPrecision(6, 2);
        b.Entity<NutritionRecord>().Property(x => x.Carbohydrates).HasPrecision(6, 2);
        b.Entity<NutritionRecord>().Property(x => x.Fat).HasPrecision(6, 2);
        b.Entity<NutritionRecord>().Property(x => x.WaterIntake).HasPrecision(6, 2);
        b.Entity<Appointment>()
            .HasOne(x => x.User)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.UserId);
        b.Entity<Appointment>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<ChatMessage>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>()
            .HasIndex(x => new { x.DoctorId, x.AppointmentDate })
            .IsUnique();
    }
}
