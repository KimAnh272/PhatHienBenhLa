namespace PhatHienBenhLa.Model
{
    public class PhatHienBenhLa
    {
        public int Id { get; set; } 
        public string TenAnh { get; set; } 
        public string LoaiCay { get; set; }
        public string LoaiBenh { get; set; } 
        public double DoTinCay { get; set; } 
        public DateTime NgayUpload { get; set; } = DateTime.Now; 
        public string NguoiDungId { get; set; } 
        public bool YeuCauChuyenGia { get; set; } = false; 
        public string? PhanHoiCuaChuyenGia { get; set; } 
        public bool BaoCaoAdmin { get; set; } = false; 
        public bool AdminDaDuyet { get; set; } = false; 
        public string? KetQuaChinhXac { get; set; } // Nếu AI sai, Admin sẽ nhập tên bệnh thật vào đây
        public string? GhiChuCuaNongDan { get; set; } // Nông dân gõ câu hỏi/nghi ngờ vào đây
    }
}
