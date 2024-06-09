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
        public async Task<ActionResult> CreateTeacher(CreateTeacherRequestDto request)
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

            // Сохранение фото
            string photoFileName = null;
            if (!string.IsNullOrEmpty(request.Photo) && IsValidBase64(request.Photo))
            {
                var (fileData, extension) = ExtractFileDataAndExtension(request.Photo);
                photoFileName = $"teacher_photo_{userId}_{Guid.NewGuid()}.{extension}";
                string uploadsFolder = Path.Combine("Uploads", "teacher_photos");
                string filePath = Path.Combine(uploadsFolder, photoFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                photoFileName = $"{Request.Scheme}://{Request.Host}/uploads/teacher_photos/{photoFileName}";
            }

            // Создание преподавателя
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

            return CreatedAtAction(nameof(CreateTeacher), new { id = teacherId }, request);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            // Проверка существования преподавателя
            var exists = await _context.CheckIfTeacherExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            // Получение идентификатора пользователя
            var userId = await _context.GetUserIdByTeacherIdAsync(id);
            if (userId == null)
            {
                return NotFound("User for the teacher not found.");
            }

            // Удаление пользователя (что приведет к каскадному удалению учителя)
            await _context.DeleteUserAsync(userId.Value);

            return NoContent();
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherRequestDto request)
        {
            var exists = await _context.CheckIfTeacherExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            string photoPath = null;
            if (!string.IsNullOrEmpty(request.PhotoData) && IsValidBase64(request.PhotoData))
            {
                var (fileData, extension) = ExtractFileDataAndExtension(request.PhotoData);
                var fileName = $"teacher_photo_{id}_{Guid.NewGuid()}.{extension}";
                photoPath = Path.Combine("Uploads", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(photoPath));
                await System.IO.File.WriteAllBytesAsync(photoPath, fileData);
                photoPath = Path.Combine($"{Request.Scheme}://{Request.Host}/uploads/", fileName);
            }

            await _context.UpdateTeacherAsync(
                id,
                request.Name,
                request.Number,
                request.PostId,
                request.Merits,
                request.RankId,
                request.DepartmentId,
                request.Visibility,
                photoPath
            );

            return NoContent();
        }

        private bool IsValidBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return false;

            base64String = base64String.Split(',')[1]; // Удаление префикса "data:image/png;base64,"
            Span<byte> buffer = new Span<byte>(new byte[base64String.Length]);
            return Convert.TryFromBase64String(base64String, buffer, out _);
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

public class UpdateTeacherRequestDto
{
    public string Name { get; set; }
    public int Number { get; set; }
    public int PostId { get; set; }
    public string Merits { get; set; }
    public int RankId { get; set; }
    public int DepartmentId { get; set; }
    public bool Visibility { get; set; }
    public string PhotoData { get; set; } // Base64 string for photo
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

