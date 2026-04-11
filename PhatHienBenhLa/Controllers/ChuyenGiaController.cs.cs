using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PhatHienBenhLa.Model;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChuyenGiaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChuyenGiaController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ========================================================
        // TẠO CLASS NÀY ĐỂ HỨNG DỮ LIỆU TỪ TRÌNH DUYỆT (CHỐNG LỖI 400 BAD REQUEST)
        // ========================================================
        public class CapNhatHoSoForm
        {
            public string? HoTen { get; set; }
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            public string? DiaChi { get; set; }
            public string? ChuyenNganh { get; set; }
            public string? DonViCongTac { get; set; }
            public IFormFile? AnhBangCap { get; set; }
        }

        // ========================================================
        // 1. API CẬP NHẬT HỒ SƠ CHUYÊN GIA
        // ========================================================
        [HttpPut("cap-nhat-ho-so/{id}")]
        public async Task<IActionResult> CapNhatHoSo(int id, [FromForm] CapNhatHoSoForm req)
        {
            var user = _context.DanhSachNguoiDung.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản!" });

            // Cập nhật thông tin (Nếu ô nào có nhập thì mới cập nhật)
            user.HoTen = req.HoTen ?? user.HoTen;
            user.Email = req.Email ?? user.Email;
            user.SoDienThoai = req.SoDienThoai ?? user.SoDienThoai;
            user.DiaChi = req.DiaChi ?? user.DiaChi;
            user.ChuyenNganh = req.ChuyenNganh ?? user.ChuyenNganh;
            user.DonViCongTac = req.DonViCongTac ?? user.DonViCongTac; // Đã đồng bộ là DonViCongTac

            // Xử lý lưu ảnh Bằng cấp
            if (req.AnhBangCap != null && req.AnhBangCap.Length > 0)
            {
                var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "bangcap");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(req.AnhBangCap.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await req.AnhBangCap.CopyToAsync(stream);
                }

                user.AnhBangCap = "/uploads/bangcap/" + fileName;
            }

            _context.SaveChanges();
            return Ok(new { message = "Cập nhật hồ sơ thành công!" });
        }


        // ========================================================
        // CÁC HÀM CŨ GIỮ NGUYÊN (GIẢI ĐÁP, PHÊ DUYỆT...)
        // ========================================================
        [HttpGet("yeu-cau-cho-xu-ly")]
        public IActionResult LayYeuCauCho()
        {
            var danhSach = _context.Set<Model.PhatHienBenhLa>()
                .Where(x => x.YeuCauChuyenGia == true && string.IsNullOrEmpty(x.PhanHoiCuaChuyenGia))
                .OrderByDescending(x => x.NgayUpload).ToList();
            return Ok(danhSach);
        }

        [HttpGet("yeu-cau-da-xu-ly")]
        public IActionResult LayYeuCauDaXuLy()
        {
            var danhSach = _context.Set<Model.PhatHienBenhLa>()
                .Where(x => x.YeuCauChuyenGia == true && !string.IsNullOrEmpty(x.PhanHoiCuaChuyenGia))
                .OrderByDescending(x => x.NgayUpload).ToList();
            return Ok(danhSach);
        }

        public class TraLoiRequest { public string NoiDung { get; set; } }

        [HttpPut("tra-loi/{idLichSu}")]
        public IActionResult TraLoi(int idLichSu, [FromBody] TraLoiRequest req)
        {
            var lichSu = _context.Set<Model.PhatHienBenhLa>().FirstOrDefault(x => x.Id == idLichSu);
            if (lichSu == null) return NotFound(new { message = "Không tìm thấy dữ liệu ảnh này!" });

            lichSu.PhanHoiCuaChuyenGia = req.NoiDung;
            _context.SaveChanges();
            return Ok(new { message = "Đã gửi tư vấn thành công cho Nông dân!" });
        }

        [HttpGet("thong-ke-du-lieu")]
        public IActionResult ThongKeDuLieu()
        {
            var data = _context.Set<Model.DongGopAnh>().ToList();
            var thongKeNhan = data.GroupBy(x => new { x.LoaiCay, x.NhanBenh })
                .Select(g => new { LoaiCay = g.Key.LoaiCay, NhanBenh = g.Key.NhanBenh, SoLuong = g.Count(), DaDuyet = g.Count(x => x.TrangThaiDuyet == 1) })
                .OrderByDescending(x => x.SoLuong).ToList();

            var thongKeNguoiDung = data.GroupBy(x => x.NguoiDungId)
                .Select(g => new { NguoiDungId = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong).Take(5).ToList();

            return Ok(new { TongSoAnh = data.Count, ThongKeNhan = thongKeNhan, ThongKeNguoiDung = thongKeNguoiDung });
        }

        [HttpGet("anh-cho-duyet")]
        public IActionResult LayAnhChoDuyet()
        {
            var danhSach = _context.Set<Model.DongGopAnh>().Where(x => x.TrangThaiDuyet == 0).OrderBy(x => x.NgayDongGop).ToList();
            return Ok(danhSach);
        }

        public class DuyetAnhRequest { public int TrangThai { get; set; } public string? LyDo { get; set; } }

        [HttpPut("duyet-anh/{id}")]
        public IActionResult DuyetAnh(int id, [FromBody] DuyetAnhRequest req)
        {
            var anh = _context.Set<Model.DongGopAnh>().FirstOrDefault(x => x.Id == id);
            if (anh == null) return NotFound();

            anh.TrangThaiDuyet = req.TrangThai;
            anh.LyDoTuChoi = req.LyDo;
            _context.SaveChanges();

            return Ok(new { message = req.TrangThai == 1 ? "Đã phê duyệt! Ảnh sẵn sàng đưa vào huấn luyện." : "Đã từ chối ảnh!" });
        }
    }
}