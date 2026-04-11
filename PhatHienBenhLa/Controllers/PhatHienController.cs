using Microsoft.AspNetCore.Mvc;
using PhatHienBenhLa.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhatHienController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhatHienController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ========================================================
        // TẠO CLASS NÀY ĐỂ HỨNG DỮ LIỆU TỪ TRÌNH DUYỆT 
        // ========================================================
        public class CapNhatNongDanForm
        {
            public string? HoTen { get; set; }
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            public string? DiaChi { get; set; }
        }

        // ========================================================
        // 0. API CẬP NHẬT HỒ SƠ NÔNG DÂN 
        // ========================================================
        [HttpPut("cap-nhat-ho-so-nong-dan/{id}")]
        public IActionResult CapNhatHoSoNongDan(int id, [FromForm] CapNhatNongDanForm req)
        {
            // Tìm nông dân trong bảng DanhSachNguoiDung
            var user = _context.DanhSachNguoiDung.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản!" });

            // Ghi đè dữ liệu mới (chỉ ghi đè nếu có nhập liệu)
            user.HoTen = req.HoTen ?? user.HoTen;
            user.Email = req.Email ?? user.Email;
            user.SoDienThoai = req.SoDienThoai ?? user.SoDienThoai;
            user.DiaChi = req.DiaChi ?? user.DiaChi;

            // Lưu vào SQL Database
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật hồ sơ nông dân thành công!" });
        }

        // ========================================================
        // 1. API NHẬN ẢNH VÀ PHÂN TÍCH AI
        // ========================================================
        [HttpPost("phan-tich")]
        public async Task<IActionResult> PhanTichAnh([FromForm] IFormFile hinhAnh, [FromForm] string nguoiDungId, [FromForm] string loaiCay)
        {
            // 1. Kiểm tra đầu vào
            if (hinhAnh == null || hinhAnh.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn ảnh!" });

            if (string.IsNullOrEmpty(loaiCay))
                return BadRequest(new { message = "Vui lòng chọn loại cây trước khi phân tích!" });

            // 2. LƯU ẢNH VÀO SERVER (Mục chẩn đoán)
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "chandoan");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await hinhAnh.CopyToAsync(stream);
            }

            string duongDanAnhDB = "/uploads/chandoan/" + fileName;

            // 3. GIẢ LẬP MÔ HÌNH AI (MOCK DATA) THEO LOẠI CÂY
            string ketQuaNhanDien = "";
            double doTinCay = 0.0;
            Random rnd = new Random();

            // Random độ tin cậy từ 75.0% đến 99.9%
            doTinCay = Math.Round(rnd.NextDouble() * (0.999 - 0.750) + 0.750, 3);

            if (loaiCay == "San")
            {
                string[] nhanSan = { "Khỏe mạnh (Healthy)", "Bệnh khảm lá (Cassava Mosaic Disease)", "Bệnh sọc nâu (Cassava Brown Streak Disease)", "Bệnh cháy lá vi khuẩn (Cassava Bacterial Blight)", "Bệnh đốm xanh (Cassava Green Mottle)" };
                ketQuaNhanDien = nhanSan[rnd.Next(nhanSan.Length)];
            }
            else if (loaiCay == "KhoaiTay")
            {
                string[] nhanKhoaiTay = { "Khỏe mạnh (Healthy)", "Vi khuẩn (Bacteria)", "Nấm (Fungi - Early blight)", "Mốc sương (Phytophthora - Late blight)", "Tuyến trùng (Nematode)", "Sâu/Côn trùng (Pest)" };
                ketQuaNhanDien = nhanKhoaiTay[rnd.Next(nhanKhoaiTay.Length)];
            }
            else
            {
                return BadRequest(new { message = "Loại cây không hợp lệ trong hệ thống!" });
            }

            // 4. LƯU KẾT QUẢ VÀO DATABASE ĐỂ HIỆN TRONG LỊCH SỬ
            var lichSuMoi = new Model.PhatHienBenhLa
            {
                NguoiDungId = nguoiDungId,
                TenAnh = duongDanAnhDB,
                LoaiCay = loaiCay,
                LoaiBenh = ketQuaNhanDien,
                DoTinCay = doTinCay,
                NgayUpload = DateTime.Now,
                YeuCauChuyenGia = false,
                BaoCaoAdmin = false
            };

            _context.Set<Model.PhatHienBenhLa>().Add(lichSuMoi);
            await _context.SaveChangesAsync();

            // 5. TRẢ KẾT QUẢ VỀ FRONTEND
            return Ok(new
            {
                message = "Phân tích thành công!",
                idLichSu = lichSuMoi.Id, // Đã có ID thật từ Database thay vì số 999
                loaiBenh = ketQuaNhanDien,
                doTinCay = doTinCay
            });
        }

        // ========================================================
        // 2. LẤY DANH SÁCH LỊCH SỬ
        // ========================================================
        [HttpGet("lich-su/{nguoiDungId}")]
        public IActionResult LayLichSu(string nguoiDungId)
        {
            var lichSu = _context.Set<Model.PhatHienBenhLa>()
                .Where(ls => ls.NguoiDungId == nguoiDungId)
                .OrderByDescending(ls => ls.NgayUpload)
                .ToList();

            return Ok(lichSu);
        }

        // ========================================================
        // 3. YÊU CẦU CHUYÊN GIA HỖ TRỢ
        // ========================================================
        public class YeuCauRequest
        {
            public string? GhiChu { get; set; }
        }

        [HttpPut("yeu-cau-ho-tro/{idLichSu}")]
        public IActionResult YeuCauChuyenGia(int idLichSu, [FromBody] YeuCauRequest req)
        {
            var lichSu = _context.Set<Model.PhatHienBenhLa>().FirstOrDefault(ls => ls.Id == idLichSu);
            if (lichSu == null)
                return NotFound(new { message = "Không tìm thấy lịch sử phân tích này!" });

            lichSu.YeuCauChuyenGia = true;
            lichSu.GhiChuCuaNongDan = req.GhiChu;
            _context.SaveChanges();

            return Ok(new { message = "Đã gửi yêu cầu và câu hỏi đến chuyên gia thành công!" });
        }

        // ========================================================
        // 4. BÁO CÁO ADMIN (AI SAI)
        // ========================================================
        [HttpPut("bao-cao-admin/{idLichSu}")]
        public IActionResult BaoCaoAdmin(int idLichSu)
        {
            var lichSu = _context.Set<Model.PhatHienBenhLa>().FirstOrDefault(ls => ls.Id == idLichSu);
            if (lichSu == null) return NotFound();

            lichSu.BaoCaoAdmin = true;
            lichSu.AdminDaDuyet = false;
            _context.SaveChanges();

            return Ok(new { message = "Đã báo cáo lỗi AI cho Quản trị viên để kiểm duyệt!" });
        }

        // ========================================================
        // 5. API LƯU ẢNH ĐÓNG GÓP
        // ========================================================
        [HttpPost("dong-gop-anh")]
        public async Task<IActionResult> DongGopAnh([FromForm] IFormFile hinhAnh, [FromForm] string nguoiDungId, [FromForm] string loaiCay, [FromForm] string nhanBenh)
        {
            if (hinhAnh == null || hinhAnh.Length == 0) return BadRequest(new { message = "Vui lòng chọn ảnh!" });

            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "donggop");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await hinhAnh.CopyToAsync(stream);
            }

            var dongGopMoi = new Model.DongGopAnh
            {
                NguoiDungId = nguoiDungId,
                TenAnh = "/uploads/donggop/" + fileName,
                LoaiCay = loaiCay,
                NhanBenh = nhanBenh,
                NgayDongGop = DateTime.Now // Lưu thời gian đóng góp
            };

            _context.Set<Model.DongGopAnh>().Add(dongGopMoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cảm ơn bạn đã đóng góp! Ảnh đang chờ chuyên gia phê duyệt." });
        }

        // ========================================================
        // 6. API LẤY LỊCH SỬ VÀ THỐNG KÊ ĐÓNG GÓP
        // ========================================================
        [HttpGet("lich-su-dong-gop/{nguoiDungId}")]
        public IActionResult LayLichSuDongGop(string nguoiDungId)
        {
            var danhSach = _context.Set<Model.DongGopAnh>()
                .Where(x => x.NguoiDungId == nguoiDungId)
                .OrderByDescending(x => x.NgayDongGop)
                .ToList();

            var thongKe = danhSach.GroupBy(x => new { x.LoaiCay, x.NhanBenh })
                                  .Select(g => $"Đã tải lên {g.Count()} ảnh {g.Key.LoaiCay} - {g.Key.NhanBenh}").ToList();

            return Ok(new { lsDongGop = danhSach, thongKe = thongKe });
        }
        // Top đóng góp
        [HttpGet("top-dong-gop")]
        public IActionResult GetTopDongGop()
        {
            // 1. Lọc ra các đóng góp ĐÃ ĐƯỢC DUYỆT (TrangThaiDuyet = 1)
            // Lưu ý: Đổi tên bảng "DanhSachDongGop" thành tên bảng chứa ảnh đóng góp trong DbContext của bạn.
            var topUsers = _context.Set<DongGopAnh>() // <--- SỬA TÊN MODEL NÀY NẾU CẦN
                .Where(d => d.TrangThaiDuyet == 1)
                .GroupBy(d => d.NguoiDungId)
                .Select(g => new {
                    NguoiDungId = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5) // Lấy Top 5 người cao nhất
                .ToList();

            // 2. Lấy Họ Tên từ bảng NguoiDung và gán Danh hiệu (Huy hiệu)
            var result = topUsers.Select(t => {
                int userId = Convert.ToInt32(t.NguoiDungId);

                // Tìm thông tin người dùng trong DB bằng biến userId vừa ép kiểu
                var user = _context.Set<NguoiDung>().FirstOrDefault(n => n.Id == userId);
                string hoTen = user != null ? user.HoTen : "Nông dân ẩn danh";

                // Gán danh hiệu dựa trên số ảnh đã đóng góp
                string danhHieu = "Mầm Non 🌱";
                if (t.SoLuong >= 50) danhHieu = "Đại Sứ Nông Nghiệp 👑";
                else if (t.SoLuong >= 20) danhHieu = "Bàn Tay Vàng 🌳";
                else if (t.SoLuong >= 5) danhHieu = "Nông Dân Chăm Chỉ 🌿";

                return new
                {
                    NguoiDungId = t.NguoiDungId,
                    HoTen = hoTen,          // TRẢ VỀ HỌ TÊN ĐẦY ĐỦ
                    SoLuong = t.SoLuong,
                    DanhHieu = danhHieu     // TRẢ VỀ HUY HIỆU TÍCH LŨY
                };
            }).ToList();

            return Ok(result);
        }
    }
}