using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace i_am_building_a_simple_graduation.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateLabResultImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "LabResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "LabResults",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "LabResults");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "LabResults");
        }
    }
}
