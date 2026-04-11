using Microsoft.AspNetCore.Mvc;
using PhatHienBenhLa.Model;
using System;
using System.Linq;

namespace PhatHienBenhLa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinNhanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TinNhanController(AppDbContext context)
        {
            _context = context;
        }

        // 1. API GỬI TIN NHẮN (Nhận từ trang Đăng nhập / Nông dân)
        [HttpPost("gui-tin")]
        public IActionResult GuiTin([FromBody] TinNhan req)
        {
            req.NgayGui = DateTime.Now;
            _context.Set<TinNhan>().Add(req);
            _context.SaveChanges();

            return Ok(new { message = "Gửi tin nhắn thành công!" });
        }

        // 2. API LẤY DANH SÁCH CHO ADMIN XEM
        [HttpGet("danh-sach")]
        public IActionResult LayDanhSach()
        {
            var danhSach = _context.Set<TinNhan>()
                .OrderByDescending(t => t.NgayGui)
                .ToList();

            return Ok(danhSach);
        }
        // 3. API ĐÁNH DẤU ĐÃ ĐỌC
        [HttpPut("danh-dau-da-doc/{id}")]
        public IActionResult DanhDauDaDoc(int id)
        {
            var msg = _context.Set<TinNhan>().FirstOrDefault(t => t.Id == id);
            if (msg != null)
            {
                msg.DaDoc = true; // Chuyển trạng thái thành đã xem
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }
    }
}