using Microsoft.AspNetCore.Mvc;
using PhatHienBenhLa.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinTucController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TinTucController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. Chuyên gia gửi bài viết (Dùng FromForm để nhận được cả file ảnh)
        [HttpPost("dang-bai")]
        public async Task<IActionResult> DangBai([FromForm] string TieuDe, [FromForm] string NoiDung, [FromForm] string LoaiCay, [FromForm] int ChuyenGiaId, [FromForm] string TenChuyenGia, [FromForm] IFormFile? AnhMinhHoa)
        {
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "tintuc");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string duongDanAnh = "";
            if (AnhMinhHoa != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AnhMinhHoa.FileName);
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhMinhHoa.CopyToAsync(stream);
                }
                duongDanAnh = "/uploads/tintuc/" + fileName;
            }

            var bv = new BaiViet
            {
                TieuDe = TieuDe,
                NoiDung = NoiDung,
                LoaiCay = LoaiCay,
                ChuyenGiaId = ChuyenGiaId,
                TenChuyenGia = TenChuyenGia,
                AnhMinhHoa = duongDanAnh,
                NgayDang = DateTime.Now,
                TrangThaiDuyet = 0
            };

            _context.Set<BaiViet>().Add(bv);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Gửi bài thành công, vui lòng chờ Admin duyệt!" });
        }

        // 2. Lấy danh sách bài viết cho Admin duyệt
        [HttpGet("danh-sach-cho-duyet")]
        public IActionResult LayBaiChoDuyet()
        {
            var ds = _context.Set<BaiViet>().Where(x => x.TrangThaiDuyet == 0).OrderByDescending(x => x.NgayDang).ToList();
            return Ok(ds);
        }

        // 3. Admin duyệt bài viết
        [HttpPut("duyet-bai/{id}")]
        public IActionResult DuyetBai(int id)
        {
            var bv = _context.Set<BaiViet>().Find(id);
            if (bv == null) return NotFound();
            bv.TrangThaiDuyet = 1;
            _context.SaveChanges();
            return Ok(new { message = "Bài viết đã được đăng công khai!" });
        }

        // 4. Lấy bài viết cho Nông dân đọc (Chỉ lấy bài đã duyệt)
        [HttpGet("danh-sach-cong-khai")]
        public IActionResult LayBaiCongKhai()
        {
            var ds = _context.Set<BaiViet>().Where(x => x.TrangThaiDuyet == 1).OrderByDescending(x => x.NgayDang).ToList();
            return Ok(ds);
        }

        // 5. Chuyên gia xem lại bài viết của chính mình
        [HttpGet("bai-viet-cua-toi/{chuyenGiaId}")]
        public IActionResult LayBaiCuaToi(int chuyenGiaId)
        {
            var ds = _context.Set<BaiViet>().Where(x => x.ChuyenGiaId == chuyenGiaId).OrderByDescending(x => x.NgayDang).ToList();
            return Ok(ds);
        }



        // API: Lấy TẤT CẢ bài viết (Cho Admin xem lịch sử)
        [HttpGet("tat-ca-bai-viet")]
        public IActionResult GetAllBaiViet()
        {
            var list = _context.BaiViets.OrderByDescending(x => x.NgayDang).ToList();
            return Ok(list);
        }

        // API: Từ chối bài viết
        [HttpPut("tu-choi-bai/{id}")]
        public IActionResult TuChoiBaiViet(int id)
        {
            var bai = _context.BaiViets.Find(id);
            if (bai == null) return NotFound();

            bai.TrangThaiDuyet = -1;

            _context.SaveChanges();
            return Ok();
        }

        // API: Thu hồi bài viết đã xuất bản
        [HttpPut("thu-hoi-bai/{id}")]
        public IActionResult ThuHoiBaiViet(int id)
        {
            var bai = _context.Set<BaiViet>().Find(id);
            if (bai == null) return NotFound();

            bai.TrangThaiDuyet = -2; // Đã Thu Hồi

            _context.SaveChanges();
            return Ok();
        }
    }
}