using System;

namespace PhatHienBenhLa.Model
{
    public class DongGopAnh
    {
        public int Id { get; set; }
        public string NguoiDungId { get; set; }
        public string TenAnh { get; set; }
        public string LoaiCay { get; set; } // Ví dụ: Lá sắn, Lá cà chua...
        public string NhanBenh { get; set; } // Nhãn bệnh do Nông dân tự gán
        public DateTime NgayDongGop { get; set; } = DateTime.Now;
        public int TrangThaiDuyet { get; set; } = 0; // 0: Chờ duyệt, 1: Đã duyệt, -1: Từ chối
        public string? LyDoTuChoi { get; set; } // Chuyên gia nhập lý do nếu ảnh mờ/sai
    }
}