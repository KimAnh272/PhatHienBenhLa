using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotChuyenGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhanHoiCuaChuyenGia",
                table: "LichSuPhatHien",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "YeuCauChuyenGia",
                table: "LichSuPhatHien",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhanHoiCuaChuyenGia",
                table: "LichSuPhatHien");

            migrationBuilder.DropColumn(
                name: "YeuCauChuyenGia",
                table: "LichSuPhatHien");
        }
    }
}
