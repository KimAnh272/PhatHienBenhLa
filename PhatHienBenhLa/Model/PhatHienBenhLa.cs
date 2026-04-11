namespace PhatHienBenhLa.Model
{
    public class PhatHienBenhLa
    {
        public int Id { get; set; } // Số thứ tự ảnh
        public string TenAnh { get; set; } // Tên file ảnh
        public string LoaiCay { get; set; }
        public string LoaiBenh { get; set; } // Kết quả AI trả về (ví dụ: Khảm lá)
        public double DoTinCay { get; set; } // Xác suất % (ví dụ: 0.95)
        public DateTime NgayUpload { get; set; } = DateTime.Now; // Thời gian thực hiện
        public string NguoiDungId { get; set; } // Ai là người gửi ảnh này
        public bool YeuCauChuyenGia { get; set; } = false; // Mặc định là không yêu cầu
        public string? PhanHoiCuaChuyenGia { get; set; } // Lời chẩn đoán lại của chuyên gia
        public bool BaoCaoAdmin { get; set; } = false; // Nông dân nhấn nút nghi ngờ AI sai
        public bool AdminDaDuyet { get; set; } = false; // Trạng thái Admin đã xử lý chưa
        public string? KetQuaChinhXac { get; set; } // Nếu AI sai, Admin sẽ nhập tên bệnh thật vào đây
        public string? GhiChuCuaNongDan { get; set; } // Nông dân gõ câu hỏi/nghi ngờ vào đây
    }
}
