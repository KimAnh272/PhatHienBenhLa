using System;

namespace PhatHienBenhLa.Model
{
    public class BaiVietHoiDap
    {
        public int Id { get; set; }
        public string NguoiDungId { get; set; }
        public string TenNguoiDang { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string? HinhAnhLBenh { get; set; } 

        public DateTime NgayDang { get; set; } = DateTime.Now;

        public int LuotXem { get; set; } = 0;
    }
}
