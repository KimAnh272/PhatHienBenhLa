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

            // 3. GỌI SANG MÔ HÌNH AI PYTHON THẬT
            string ketQuaNhanDien = "Không xác định";
            double doTinCay = 0.0;

            string modelType = (loaiCay == "KhoaiTay") ? "YOLO" : "RESNET";

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        // Đọc file ảnh vừa lưu để gửi sang Python
                        using (var stream = new FileStream(filePath, FileMode.Open))
                        {
                            var fileContent = new StreamContent(stream);
                            fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(hinhAnh.ContentType);

                            content.Add(fileContent, "file", hinhAnh.FileName);
                            content.Add(new StringContent(modelType), "model_type"); // Báo cho Python biết dùng mô hình nào
                            content.Add(new StringContent(loaiCay), "loai_cay");     // Báo cho Python biết là lá Sắn hay Khoai Tây

                            // Bắn dữ liệu sang cổng 8000 của Python
                            var response = await client.PostAsync("http://localhost:8000/predict", content);

                            if (response.IsSuccessStatusCode)
                            {
                                var resultStr = await response.Content.ReadAsStringAsync();

                                // Đọc file JSON do Python trả về
                                using (var jsonDoc = System.Text.Json.JsonDocument.Parse(resultStr))
                                {
                                    ketQuaNhanDien = jsonDoc.RootElement.GetProperty("loaiBenh").GetString();
                                    doTinCay = jsonDoc.RootElement.GetProperty("doTinCay").GetDouble();
                                }
                            }
                            else
                            {
                                return BadRequest(new { message = "Lỗi từ AI Server: Quá trình phân tích thất bại." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Không thể kết nối đến AI Server! Vui lòng kiểm tra lại Python. Lỗi: " + ex.Message });
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
        [HttpPut("bao-cao-sai/{id}")]
        public IActionResult BaoCaoAISai(int id)
        {
            try
            {
                var lichSu = _context.PhatHienBenhLa.FirstOrDefault(x => x.Id == id);
                if (lichSu == null)
                {
                    return NotFound(new { message = "Không tìm thấy lịch sử phân tích này!" });
                }

                // Bật cờ báo cáo sai lên true để đẩy sang tab Phê duyệt của Chuyên gia/Admin
                lichSu.BaoCaoAdmin = true;

                // Nếu bảng của bạn có cột TrangThaiDuyet, hãy bật nó về 0 (Chờ duyệt)
                // lichSu.TrangThaiDuyet = 0; 

                _context.SaveChanges();

                return Ok(new { message = "Báo cáo thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi máy chủ: " + ex.Message });
            }
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
                NgayDongGop = DateTime.Now, // Lưu thời gian đóng góp
                TenChuyenGiaDuyet = ""
            };

            _context.Set<Model.DongGopAnh>().Add(dongGopMoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cảm ơn bạn đã đóng góp! Ảnh đang chờ chuyên gia phê duyệt." });
        }

        // ========================================================
        // 6. API LẤY LỊCH SỬ VÀ THỐNG KÊ ĐÓNG GÓP
        // ========================================================
        [HttpGet("lich-su-dong-gop/{userId}")]
        public IActionResult LayLichSuDongGop(string userId)
        {
            try
            {
                var danhSach = _context.Set<Model.DongGopAnh>()
                    .Where(x => x.NguoiDungId == userId)
                    .OrderByDescending(x => x.NgayDongGop)
                    .ToList();

                var tongSo = danhSach.Count;
                var daDuyet = danhSach.Count(x => x.TrangThaiDuyet == 1);
                var choDuyet = danhSach.Count(x => x.TrangThaiDuyet == 0);
                var tuChoi = danhSach.Count(x => x.TrangThaiDuyet == -1);

                var thongKe = new List<string>
                {
                    $"Tổng số ảnh đã gửi: {tongSo}",
                    $"Đã được phê duyệt: {daDuyet}",
                    $"Đang chờ duyệt: {choDuyet}",
                    $"Bị từ chối: {tuChoi}"
                };

                // Trả về dữ liệu chuẩn khớp với tên biến Javascript mong đợi
                var lsDongGop = danhSach.Select(x => new
                {
                    id = x.Id,
                    tenAnh = x.TenAnh,
                    loaiCay = x.LoaiCay, // Gửi về để Javascript có thể Filter
                    nhanBenh = x.NhanBenh,
                    trangThaiDuyet = x.TrangThaiDuyet,
                    lyDoTuChoi = x.LyDoTuChoi,
                    ngayDongGop = x.NgayDongGop
                }).ToList();

                return Ok(new
                {
                    thongKe = thongKe,
                    lsDongGop = lsDongGop
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy lịch sử đóng góp: " + ex.Message });
            }
        }

        // Top đóng góp
        [HttpGet("top-dong-gop")]
        public IActionResult GetTopDongGop()
        {
            // 1. Lọc ra các đóng góp ĐÃ ĐƯỢC DUYỆT (TrangThaiDuyet = 1)
            var topUsers = _context.Set<DongGopAnh>() //
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
                    HoTen = hoTen,    
                    SoLuong = t.SoLuong,
                    DanhHieu = danhHieu
                };
            }).ToList();

            return Ok(result);
        }

        [HttpPost("test-mo-hinh")]
        public async Task<IActionResult> TestMoHinhAI([FromForm] IFormFile hinhAnh, [FromForm] string loaiCay, [FromForm] string model_type)
        {
            if (hinhAnh == null || hinhAnh.Length == 0) return BadRequest("Chưa có ảnh!");

            try
            {
                using (var client = new HttpClient())
                {
                    // 1. Chuẩn bị dữ liệu để gửi sang Server Python (cổng 5000)
                    var content = new MultipartFormDataContent();

                    // Chuyển file ảnh sang byte để gửi đi
                    var stream = hinhAnh.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    content.Add(fileContent, "hinhAnh", hinhAnh.FileName);

                    // Gửi kèm loại cây và loại mô hình
                    content.Add(new StringContent(loaiCay), "loaiCay");
                    content.Add(new StringContent(model_type), "model_type");

                    // 2. Gọi sang Server Python (Địa chỉ mà bạn thấy ở CMD lúc nãy)
                    var response = await client.PostAsync("http://127.0.0.1:5000/predict", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        // Trả kết quả nhận được từ Python về lại cho giao diện Admin.html
                        return Content(result, "application/json");
                    }
                    else
                    {
                        return StatusCode(500, "Server AI Python không phản hồi hoặc bị lỗi!");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}