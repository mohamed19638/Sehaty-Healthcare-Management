using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace i_am_building_a_simple_graduation.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAndMeasurementContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fail with an actionable message rather than deleting appointments
            // if legacy records already contain duplicate doctor/time slots.
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [Appointments] GROUP BY [DoctorId], [AppointmentDate] HAVING COUNT(*) > 1) THROW 51000, 'Cannot add the unique appointment slot constraint because duplicate doctor/time appointments already exist. Resolve those records without deleting user data, then retry the migration.', 1;");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppPhone",
                table: "Pharmacies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BloodSugarWasFasting",
                table: "HealthMeasurements",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HeartRateWasAtRest",
                table: "HealthMeasurements",
                type: "bit",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CaloriesBurned",
                table: "FitnessActivities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "YoutubeUrl",
                table: "FitnessActivities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppPhone",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YoutubeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Calories = table.Column<int>(type: "int", nullable: true),
                    IsEstimate = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Meals");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "WhatsAppPhone",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "BloodSugarWasFasting",
                table: "HealthMeasurements");

            migrationBuilder.DropColumn(
                name: "HeartRateWasAtRest",
                table: "HealthMeasurements");

            migrationBuilder.DropColumn(
                name: "YoutubeUrl",
                table: "FitnessActivities");

            migrationBuilder.DropColumn(
                name: "WhatsAppPhone",
                table: "Doctors");

            migrationBuilder.AlterColumn<int>(
                name: "CaloriesBurned",
                table: "FitnessActivities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");
        }
    }
}
