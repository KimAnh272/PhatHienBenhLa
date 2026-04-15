using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace PhatHienBenhLa.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tạo bảng chứa thông tin phát hiện bệnh
        public DbSet<PhatHienBenhLa> LichSuPhatHien { get; set; }
        public DbSet<Model.PhatHienBenhLa> PhatHienBenhLa { get; set; }

        // Tạo bảng chứa thông tin người dùng
        public DbSet<NguoiDung> DanhSachNguoiDung { get; set; }
        public DbSet<TinNhan> DanhSachTinNhan { get; set; }
        public DbSet<DongGopAnh> DanhSachDongGop { get; set; }
        public DbSet<BaiViet> DanhSachBaiViet { get; set; }
        public DbSet<BaiViet> BaiViets { get; set; }
    }
}
