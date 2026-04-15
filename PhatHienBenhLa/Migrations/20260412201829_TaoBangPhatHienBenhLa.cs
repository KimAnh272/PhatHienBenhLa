using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangPhatHienBenhLa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LichSuPhatHien",
                table: "LichSuPhatHien");

            migrationBuilder.RenameTable(
                name: "LichSuPhatHien",
                newName: "PhatHienBenhLa");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhatHienBenhLa",
                table: "PhatHienBenhLa",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PhatHienBenhLa",
                table: "PhatHienBenhLa");

            migrationBuilder.RenameTable(
                name: "PhatHienBenhLa",
                newName: "LichSuPhatHien");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LichSuPhatHien",
                table: "LichSuPhatHien",
                column: "Id");
        }
    }
}
