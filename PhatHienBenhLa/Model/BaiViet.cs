using System;

namespace PhatHienBenhLa.Model
{
    public class BaiViet
    {
        public int Id { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; } 
        public string? AnhMinhHoa { get; set; }
        public string LoaiCay { get; set; } 
        public int ChuyenGiaId { get; set; }
        public string TenChuyenGia { get; set; }
        public DateTime NgayDang { get; set; }
        public int TrangThaiDuyet { get; set; } = 0;
    }
}