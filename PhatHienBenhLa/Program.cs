using Microsoft.EntityFrameworkCore;
using PhatHienBenhLa.Model;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// KHỞI ĐỘNG AI SERVER CHẠY CÙNG LÚC VỚI WEB

Process aiProcess = new Process();
try
{
    aiProcess.StartInfo.FileName = "cmd.exe";
    aiProcess.StartInfo.Arguments = "/c uvicorn ai_server:app --host 0.0.0.0 --port 8000";

    // Đường dẫn trỏ thẳng đến thư mục AI_Server của bạn
    aiProcess.StartInfo.WorkingDirectory = @"D:\Documents\DoAnTotNghiep\AI_Server";

    aiProcess.StartInfo.UseShellExecute = false;
    aiProcess.StartInfo.CreateNoWindow = true; // Chạy ngầm, không hiện bảng đen

    aiProcess.Start();
    Console.WriteLine("=> DA KHOI DONG AI PYTHON SERVER THANH CONG!");
}
catch (Exception ex)
{
    Console.WriteLine("=> LOI KHOI DONG AI: " + ex.Message);
}

// Tự động tắt Python khi bạn dừng chạy Web C#
app.Lifetime.ApplicationStopping.Register(() =>
{
    try { if (!aiProcess.HasExited) aiProcess.Kill(true); } catch { }
});

app.UseStaticFiles(); // Cho phép dùng file trong wwwroot
app.UseDefaultFiles(); // Tự động mở index.html khi chạy

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
