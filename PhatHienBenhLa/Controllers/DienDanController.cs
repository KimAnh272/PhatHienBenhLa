using Microsoft.AspNetCore.Mvc;
using PhatHienBenhLa.Model;
using System;
using System.Linq;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DienDanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DienDanController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. API BÀI VIẾT HỎI ĐÁP
        // ==========================================

        // Lấy danh sách bài viết mới nhất
        [HttpGet("bai-viet")]
        public IActionResult LayDanhSachBaiViet()
        {
            try
            {
                var danhSach = _context.BaiVietHoiDaps
                    .OrderByDescending(b => b.NgayDang)
                    .Select(b => new
                    {
                        b.Id,
                        b.NguoiDungId,
                        b.TenNguoiDang,
                        b.TieuDe,
                        b.NoiDung,
                        b.HinhAnhLBenh,
                        b.NgayDang,
                        b.LuotXem,
                        LuotBinhLuan = _context.BinhLuans.Count(c => c.BaiVietId == b.Id)
                    }).ToList();

                return Ok(danhSach);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        public class TaoBaiVietRequest
        {
            public string NguoiDungId { get; set; }
            public string TenNguoiDang { get; set; }
            public string TieuDe { get; set; }
            public string NoiDung { get; set; }
        }

        // Tạo bài viết mới
        [HttpPost("bai-viet")]
        public async Task<IActionResult> TaoBaiViet([FromForm] string nguoiDungId, [FromForm] string tenNguoiDang, [FromForm] string tieuDe, [FromForm] string noiDung, [FromForm] List<IFormFile> hinhAnhs)
        {
            if (string.IsNullOrEmpty(tieuDe) || string.IsNullOrEmpty(noiDung))
                return BadRequest("Tiêu đề và nội dung không được để trống!");

            string chuoiDuongDanAnh = "";

            // XỬ LÝ LƯU TỪNG ẢNH VÀO SERVER
            if (hinhAnhs != null && hinhAnhs.Count > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "diendan");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                List<string> dsDuongDan = new List<string>();
                foreach (var file in hinhAnhs)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    dsDuongDan.Add("/uploads/diendan/" + fileName);
                }
                chuoiDuongDanAnh = string.Join("|", dsDuongDan);
            }

            var baiMoi = new BaiVietHoiDap
            {
                NguoiDungId = nguoiDungId,
                TenNguoiDang = tenNguoiDang,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                HinhAnhLBenh = chuoiDuongDanAnh, 
                NgayDang = DateTime.Now,
                LuotXem = 0
            };

            _context.BaiVietHoiDaps.Add(baiMoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng bài thành công!" });
        }

        // ==========================================
        // 2. API BÌNH LUẬN (COMMENT)
        // ==========================================

        // Lấy danh sách bình luận của 1 bài viết cụ thể
        [HttpGet("binh-luan/{baiVietId}")]
        public IActionResult LayBinhLuan(int baiVietId)
        {
            var comments = _context.BinhLuans
                .Where(c => c.BaiVietId == baiVietId)
                .OrderBy(c => c.NgayBinhLuan)
                .ToList();

            return Ok(comments);
        }

        public class TaoBinhLuanRequest
        {
            public int BaiVietId { get; set; }
            public string NguoiDungId { get; set; }
            public string TenNguoiDung { get; set; }
            public string VaiTro { get; set; }
            public string NoiDung { get; set; }
        }

        // Gửi bình luận mới
        [HttpPost("binh-luan")]
        public IActionResult TaoBinhLuan([FromBody] TaoBinhLuanRequest req)
        {
            if (string.IsNullOrEmpty(req.NoiDung)) return BadRequest("Nội dung trống!");

            var cmt = new BinhLuan
            {
                BaiVietId = req.BaiVietId,
                NguoiDungId = req.NguoiDungId,
                TenNguoiDung = req.TenNguoiDung,
                VaiTro = req.VaiTro,
                NoiDung = req.NoiDung,
                NgayBinhLuan = DateTime.Now
            };

            _context.BinhLuans.Add(cmt);
            _context.SaveChanges();

            return Ok(new { message = "Đã gửi bình luận!", data = cmt });
        }

        // ==========================================
        // 3. CÁC TÍNH NĂNG NÂNG KHÁC
        // ==========================================

        // Thả tim bình luận
        [HttpPut("tha-tim-binh-luan/{id}")]
        public IActionResult ThaTimBinhLuan(int id)
        {
            var cmt = _context.BinhLuans.FirstOrDefault(c => c.Id == id);
            if (cmt == null) return NotFound();

            cmt.LuotTim += 1;
            _context.SaveChanges();
            return Ok(new { luotTim = cmt.LuotTim });
        }

        // Admin Thu hồi bình luận
        [HttpPut("thu-hoi-binh-luan/{id}")]
        public IActionResult ThuHoiBinhLuan(int id)
        {
            var cmt = _context.BinhLuans.FirstOrDefault(c => c.Id == id);
            if (cmt == null) return NotFound();

            cmt.IsRevoked = true;
            _context.SaveChanges();
            return Ok();
        }

        // Xác nhận giải pháp (Verified)
        [HttpPut("xac-nhan-giai-phap/{id}")]
        public IActionResult XacNhanGiaiPhap(int id)
        {
            var cmt = _context.BinhLuans.FirstOrDefault(c => c.Id == id);
            if (cmt == null) return NotFound();

            cmt.IsVerified = !cmt.IsVerified;
            _context.SaveChanges();
            return Ok(new { isVerified = cmt.IsVerified });
        }
    }
}