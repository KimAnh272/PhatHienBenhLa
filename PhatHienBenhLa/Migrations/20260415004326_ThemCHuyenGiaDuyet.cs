using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class ThemCHuyenGiaDuyet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChuyenGiaId",
                table: "DanhSachDongGop",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenChuyenGiaDuyet",
                table: "DanhSachDongGop",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChuyenGiaId",
                table: "DanhSachDongGop");

            migrationBuilder.DropColumn(
                name: "TenChuyenGiaDuyet",
                table: "DanhSachDongGop");
        }
    }
}
