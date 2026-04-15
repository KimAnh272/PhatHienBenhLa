using Microsoft.AspNetCore.Mvc;
using PhatHienBenhLa.Model;
using System.Linq;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. API ĐĂNG KÝ
        // ==========================================
        [HttpPost("dang-ky")]
        public IActionResult DangKy([FromBody] NguoiDung request)
        {
            // Kiểm tra xem tên đăng nhập đã có người dùng chưa
            var taiKhoanDaTonTai = _context.DanhSachNguoiDung
                .FirstOrDefault(u => u.TenDangNhap == request.TenDangNhap);

            if (taiKhoanDaTonTai != null)
            {
                return BadRequest(new { message = "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!" });
            }

            // --- TẠO ĐỐI TƯỢNG MỚI ĐỂ KIỂM SOÁT HOÀN TOÀN DỮ LIỆU ---
            var newUser = new Model.NguoiDung
            {
                HoTen = request.HoTen,
                TenDangNhap = request.TenDangNhap,
                MatKhau = request.MatKhau,
                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                DiaChi = request.DiaChi,
                VaiTro = request.VaiTro,
                ChuyenNganh = request.ChuyenNganh,
                AnhBangCap = request.AnhBangCap,

                // LOGIC DUYỆT: Nếu là Nông dân thì true (Duyệt luôn), Chuyên gia thì false (Chờ duyệt)
                TrangThaiDuyet = (request.VaiTro == "NongDan") ? true : false,
                DaBiKhoa = false
            };

            _context.DanhSachNguoiDung.Add(newUser);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Đăng ký thành công!",
                hoTen = newUser.HoTen,
                vaiTro = newUser.VaiTro,
                id = newUser.Id.ToString(),
                trangThaiDuyet = newUser.TrangThaiDuyet,// Trả về để Frontend biết đường xử lý
                chuyenNganh = newUser.ChuyenNganh
            });
        }

        // ==========================================
        // 2. API ĐĂNG NHẬP
        // ==========================================
        [HttpPost("dang-nhap")]
        public IActionResult DangNhap([FromBody] ThongTinDangNhap request)
        {
            // 1. KIỂM TRA TÀI KHOẢN ADMIN ĐẶC QUYỀN TRƯỚC TIÊN
            if (request.TenDangNhap == "admin" && request.MatKhau == "kimngoc2004")
            {
                return Ok(new
                {
                    message = "Đăng nhập quyền Admin thành công!",
                    vaiTro = "Admin",
                    hoTen = "Quản trị viên (Kim Ngọc)",
                    id = "admin_vip_01"
                });
            }

            // 2. Tìm tài khoản Nông dân hoặc Chuyên gia trong Database
            var user = _context.DanhSachNguoiDung
                .FirstOrDefault(u => u.TenDangNhap == request.TenDangNhap && u.MatKhau == request.MatKhau);

            if (user == null)
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            // --- KIỂM TRA TÀI KHOẢN CÓ BỊ KHÓA KHÔNG ---
            if (user.DaBiKhoa)
            {
                // Trả về thêm cờ isLocked để Frontend kích hoạt tính năng nhắn tin cho Admin
                return BadRequest(new
                {
                    isLocked = true,
                    userId = user.Id.ToString(),
                    hoTen = user.HoTen,
                    vaiTro = user.VaiTro,
                    message = "Tài khoản của bạn đã bị khóa!"
                });
            }

            // --- CHẶN CỬA CHUYÊN GIA CHƯA ĐƯỢC DUYỆT ---
            if (user.VaiTro == "ChuyenGia" && user.TrangThaiDuyet == false)
            {
                return BadRequest(new { message = "Tài khoản của bạn ĐANG CHỜ ADMIN XÁC MINH BẰNG CẤP. Vui lòng quay lại sau!" });
            }

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                vaiTro = user.VaiTro,
                hoTen = user.HoTen,
                id = user.Id.ToString(),
                chuyenNganh = user.ChuyenNganh
            });
        }

        // ==========================================
        // 3. API QUÊN MẬT KHẨU
        // ==========================================
        [HttpPost("quen-mat-khau")]
        public IActionResult QuenMatKhau([FromBody] ThongTinQuenMatKhau request)
        {
            var user = _context.DanhSachNguoiDung
                .FirstOrDefault(u => u.TenDangNhap == request.TenDangNhap && u.Email == request.Email);

            if (user == null)
            {
                return BadRequest(new { message = "Thông tin xác minh không chính xác hoặc tài khoản không tồn tại!" });
            }

            user.MatKhau = request.MatKhauMoi;
            _context.SaveChanges();

            return Ok(new { message = "Đặt lại mật khẩu thành công!" });
        }
    }

    // ==========================================
    // CÁC CLASS HỖ TRỢ NHẬN DỮ LIỆU TỪ TRÌNH DUYỆT
    // ==========================================
    public class ThongTinDangNhap
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
    }

    public class ThongTinQuenMatKhau
    {
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
        public string MatKhauMoi { get; set; }
    }
}