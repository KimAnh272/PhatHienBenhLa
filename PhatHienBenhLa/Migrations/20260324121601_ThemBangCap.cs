using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangCap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhBangCap",
                table: "DanhSachNguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChuyenNganh",
                table: "DanhSachNguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViCongTac",
                table: "DanhSachNguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrangThaiDuyet",
                table: "DanhSachNguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhBangCap",
                table: "DanhSachNguoiDung");

            migrationBuilder.DropColumn(
                name: "ChuyenNganh",
                table: "DanhSachNguoiDung");

            migrationBuilder.DropColumn(
                name: "DonViCongTac",
                table: "DanhSachNguoiDung");

            migrationBuilder.DropColumn(
                name: "TrangThaiDuyet",
                table: "DanhSachNguoiDung");
        }
    }
}
