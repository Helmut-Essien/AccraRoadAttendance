using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccraRoadAttendance.Migrations
{
    /// <inheritdoc />
    public partial class uniqueAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_MemberId",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MemberId_ServiceDate_ServiceType",
                table: "Attendances",
                columns: new[] { "MemberId", "ServiceDate", "ServiceType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_MemberId_ServiceDate_ServiceType",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MemberId",
                table: "Attendances",
                column: "MemberId");
        }
    }
}
