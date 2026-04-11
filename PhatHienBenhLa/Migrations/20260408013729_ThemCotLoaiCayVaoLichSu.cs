using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotLoaiCayVaoLichSu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoaiCay",
                table: "LichSuPhatHien",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoaiCay",
                table: "LichSuPhatHien");
        }
    }
}
