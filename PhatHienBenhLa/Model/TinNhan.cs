using System;

namespace PhatHienBenhLa.Model
{
    public class TinNhan
    {
        public int Id { get; set; }
        public string NguoiGuiId { get; set; }
        public string TenNguoiGui { get; set; }
        public string VaiTro { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public bool DaDoc { get; set; } = false;
    }
}