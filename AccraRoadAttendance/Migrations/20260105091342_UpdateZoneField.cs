using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccraRoadAttendance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateZoneField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "zone",
                table: "Members",
                type: "int",
                maxLength: 500,
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "zone",
                table: "Members");
        }
    }
}
