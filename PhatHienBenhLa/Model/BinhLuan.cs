using System;

namespace PhatHienBenhLa.Model
{
    public class BinhLuan
    {
        public int Id { get; set; }
        public int BaiVietId { get; set; }
        public string NguoiDungId { get; set; }
        public string TenNguoiDung { get; set; }
        public string VaiTro { get; set; } 
        public string NoiDung { get; set; }
        public DateTime NgayBinhLuan { get; set; } = DateTime.Now;
        public int LuotTim { get; set; } = 0; // Đếm số lượt thả tim
        public bool IsRevoked { get; set; } = false; // Trạng thái Admin Thu hồi
        public bool IsVerified { get; set; } = false; // Trạng thái "Giải pháp đã xác minh"
    }
}
