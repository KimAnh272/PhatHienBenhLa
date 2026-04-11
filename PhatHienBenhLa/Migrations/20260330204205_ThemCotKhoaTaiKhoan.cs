using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotKhoaTaiKhoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaBiKhoa",
                table: "DanhSachNguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaBiKhoa",
                table: "DanhSachNguoiDung");
        }
    }
}
