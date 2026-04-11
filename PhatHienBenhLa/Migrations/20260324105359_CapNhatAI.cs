using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminDaDuyet",
                table: "LichSuPhatHien",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BaoCaoAdmin",
                table: "LichSuPhatHien",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KetQuaChinhXac",
                table: "LichSuPhatHien",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminDaDuyet",
                table: "LichSuPhatHien");

            migrationBuilder.DropColumn(
                name: "BaoCaoAdmin",
                table: "LichSuPhatHien");

            migrationBuilder.DropColumn(
                name: "KetQuaChinhXac",
                table: "LichSuPhatHien");
        }
    }
}
