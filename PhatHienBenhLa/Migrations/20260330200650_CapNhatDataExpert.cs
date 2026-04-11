using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatDataExpert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GhiChuCuaNongDan",
                table: "LichSuPhatHien",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoTuChoi",
                table: "DanhSachDongGop",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChuCuaNongDan",
                table: "LichSuPhatHien");

            migrationBuilder.DropColumn(
                name: "LyDoTuChoi",
                table: "DanhSachDongGop");
        }
    }
}
