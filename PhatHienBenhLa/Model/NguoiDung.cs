namespace PhatHienBenhLa.Model
{
    public class NguoiDung
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public string VaiTro { get; set; } // "NongDan", "ChuyenGia", "Admin"
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public bool IsActive { get; set; } // Trạng thái tài khoản
        public string? ChuyenNganh { get; set; }
        public string? DonViCongTac { get; set; }
        public string? AnhBangCap { get; set; } 
        public bool TrangThaiDuyet { get; set; } = true;
        public bool DaBiKhoa { get; set; } = false; // Mặc định là false (Chưa khóa)
    }
}
