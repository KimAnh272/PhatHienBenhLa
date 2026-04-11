using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class DoiKieuTrangThaiDuyet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DanhSachBaiViet",
                table: "DanhSachBaiViet");

            migrationBuilder.RenameTable(
                name: "DanhSachBaiViet",
                newName: "BaiViet");

            migrationBuilder.AlterColumn<int>(
                name: "TrangThaiDuyet",
                table: "BaiViet",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaiViet",
                table: "BaiViet",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BaiViet",
                table: "BaiViet");

            migrationBuilder.RenameTable(
                name: "BaiViet",
                newName: "DanhSachBaiViet");

            migrationBuilder.AlterColumn<bool>(
                name: "TrangThaiDuyet",
                table: "DanhSachBaiViet",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanhSachBaiViet",
                table: "DanhSachBaiViet",
                column: "Id");
        }
    }
}
