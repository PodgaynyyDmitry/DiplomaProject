using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Data;
using ServerApp.Models;
using System;
using System.Threading.Tasks;
namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/teachers")]
    public class TeachersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeachersController(AppDbContext context)
        {
            _context = context;
            
        }

        [HttpPost]
        public async Task<ActionResult> CreateTeacher([FromBody] CreateTeacherRequestDto request)
        {
            // Создание пользователя
            int userId = await _context.CreateUserAsync(
                request.Login,
                request.Password,
                request.SessionStatus,
                request.RoleId);

            if (userId <= 0)
            {
                return BadRequest("User creation failed.");
            }

            // Сохранение фото на сервере
            string photoFileName = null;
            if (!string.IsNullOrEmpty(request.Photo))
            {
                var (fileData, extension) = ExtractFileDataAndExtension(request.Photo);
                photoFileName = $"{Guid.NewGuid()}.{extension}";
                var photoFilePath = Path.Combine("uploads", photoFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(photoFilePath));
                await System.IO.File.WriteAllBytesAsync(photoFilePath, fileData);
            }

            // Создание учителя
            int teacherId = await _context.CreateTeacherAsync(
                userId,
                request.Name,
                request.Number,
                request.PostId,
                request.Merits,
                request.RankId,
                request.DepartmentId,
                request.Visibility,
                photoFileName);

            if (teacherId <= 0)
            {
                return BadRequest("Teacher creation failed.");
            }

            return Ok(new { userId, teacherId });
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherDto>> GetTeacher(int id)
        {
            var teacher = await _context.GetTeacherAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }


        private (byte[] fileData, string extension) ExtractFileDataAndExtension(string base64String)
        {
            var parts = base64String.Split(',');
            var metaData = parts[0]; // Пример: "data:image/png;base64,"
            var base64Data = parts[1];

            var data = Convert.FromBase64String(base64Data);
            var extension = metaData.Split(';')[0].Split('/')[1]; // Пример: "png"

            return (data, extension);
        }
    }
}
public class UserDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public bool SessionStatus { get; set; }
    public int RoleId { get; set; }
}

public class TeacherDto
{
    public int TeacherId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public int Number { get; set; }
    public string Post { get; set; }
    public string Merits { get; set; }
    public string Rank { get; set; }
    public string Department { get; set; }
    public bool Visibility { get; set; }
    public string Photo { get; set; }
}
public class CreateTeacherRequestDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public bool SessionStatus { get; set; }
    public int RoleId { get; set; }
    public string Name { get; set; }
    public int Number { get; set; }
    public int PostId { get; set; }
    public string Merits { get; set; }
    public int RankId { get; set; }
    public int DepartmentId { get; set; }
    public bool Visibility { get; set; }
    public string Photo { get; set; } // Base64 encoded photo
}

