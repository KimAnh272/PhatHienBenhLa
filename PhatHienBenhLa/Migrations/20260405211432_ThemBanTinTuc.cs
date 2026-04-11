using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhatHienBenhLa.Migrations
{
    /// <inheritdoc />
    public partial class ThemBanTinTuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhSachBaiViet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhMinhHoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiCay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChuyenGiaId = table.Column<int>(type: "int", nullable: false),
                    TenChuyenGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiDuyet = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhSachBaiViet", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhSachBaiViet");
        }
    }
}
