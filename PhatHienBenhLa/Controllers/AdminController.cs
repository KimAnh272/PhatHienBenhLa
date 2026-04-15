using Microsoft.AspNetCore.Mvc;
using PhatHienBenhLa.Model;
using System.Linq;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY SỐ LIỆU THỐNG KÊ TỔNG QUAN
        [HttpGet("thong-ke")]
        public IActionResult LayThongKe()
        {
            var tongNguoiDung = _context.DanhSachNguoiDung.Count();
            var tongLuotKham = _context.Set<Model.PhatHienBenhLa>().Count();

            return Ok(new { tongNguoiDung = tongNguoiDung, tongLuotKham = tongLuotKham });
        }

        // 2. LẤY TOÀN BỘ DANH SÁCH NGƯỜI DÙNG
        [HttpGet("nguoi-dung")]
        public IActionResult LayDanhSachNguoiDung()
        {
            var danhSach = _context.DanhSachNguoiDung
                .OrderByDescending(u => u.Id)
                .Select(u => new { u.Id, u.HoTen, u.TenDangNhap, u.Email, u.VaiTro, u.TrangThaiDuyet, u.DaBiKhoa })
                .ToList();

            return Ok(danhSach);
        }

        // 3. LẤY TOÀN BỘ LỊCH SỬ CHẨN ĐOÁN
        [HttpGet("lich-su")]
        public IActionResult LayLichSuToanHeThong()
        {
            var lichSu = _context.Set<Model.PhatHienBenhLa>()
                .OrderByDescending(ls => ls.NgayUpload)
                .ToList();

            return Ok(lichSu);
        }

        // 4. XEM CHI TIẾT 1 NGƯỜI DÙNG
        [HttpGet("chi-tiet-nguoi-dung/{id}")]
        public IActionResult LayChiTietNguoiDung(int id)
        {
            var user = _context.DanhSachNguoiDung.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng!" });

            // Ẩn mật khẩu trước khi gửi về Frontend để bảo mật
            user.MatKhau = "********";
            return Ok(user);
        }

        // 5. DUYỆT CHUYÊN GIA
        [HttpPut("duyet-chuyen-gia/{id}")]
        public IActionResult DuyetChuyenGia(int id)
        {
            var user = _context.DanhSachNguoiDung.FirstOrDefault(u => u.Id == id);

            if (user != null && user.VaiTro == "ChuyenGia")
            {
                user.TrangThaiDuyet = true; 
                _context.SaveChanges();    

                return Ok(new { message = "Đã duyệt thành công!" });
            }
            return BadRequest(new { message = "Không tìm thấy chuyên gia này hoặc lỗi dữ liệu!" });
        }

        // 6. KHÓA TÀI KHOẢN
        [HttpPut("khoa-tai-khoan/{id}")]
        public IActionResult KhoaTaiKhoan(int id)
        {
            var user = _context.DanhSachNguoiDung.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.DaBiKhoa = true;       
                _context.SaveChanges();     
                return Ok();
            }
            return BadRequest();
        }

        // 7. MỞ KHÓA TÀI KHOẢN
        [HttpPut("mo-khoa-tai-khoan/{id}")]
        public IActionResult MoKhoaTaiKhoan(int id)
        {
            var user = _context.DanhSachNguoiDung.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.DaBiKhoa = false;  
                _context.SaveChanges(); 
                return Ok();
            }
            return BadRequest();
        }
        // 8. LẤY DỮ LIỆU SẠCH ĐÃ ĐƯỢC CHUYÊN GIA DUYỆT ĐỂ HUẤN LUYỆN AI
        [HttpGet("du-lieu-huan-luyen")]
        public IActionResult GetDuLieuHuanLuyen()
        {
            var data = _context.DanhSachDongGop
                .Where(x => x.TrangThaiDuyet == 1)
                .Select(x => new
                {
                    id = x.Id,
                    tenAnh = x.TenAnh,
                    nhanBenh = x.NhanBenh,
                    loaiCay = x.LoaiCay,
                    nguoiDungId = x.NguoiDungId,
                    ngayDongGop = x.NgayDongGop,
                    chuyenGiaId = x.ChuyenGiaId,
                    tenChuyenGiaDuyet = x.TenChuyenGiaDuyet
                })
                .ToList();

            return Ok(data);
        }
    }
}