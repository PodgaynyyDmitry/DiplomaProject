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
    [Route("api/platoons")]
    public class PlatoonsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlatoonsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePlatoon([FromBody] PlatoonDto platoonDto)
        {
            int platoonId = await _context.CreatePlatoonAsync(
                platoonDto.Name,
                platoonDto.Status,
                platoonDto.VisitDayId,
                platoonDto.DepartmentId
            );

            if (platoonId <= 0)
            {
                return BadRequest("Platoon creation failed.");
            }

            return CreatedAtAction(nameof(CreatePlatoon), new { id = platoonId }, platoonDto);
        }

        [HttpPost("students")]
        public async Task<ActionResult> CreateStudent([FromBody] CreateStudentRequestDto request)
        {
            // Создание пользователя
            int userId = await _context.CreateUserAsync(request.Login, request.Password, request.SessionStatus, request.RoleId);

            if (userId <= 0)
            {
                return BadRequest("User creation failed.");
            }

            // Создание студента
            int studentId = await _context.CreateStudentAsync(userId, request.PlatoonsId, request.SequenceNumber);

            if (studentId <= 0)
            {
                return BadRequest("Student creation failed.");
            }

            return CreatedAtAction(nameof(CreateStudent), new { id = studentId }, request);
        }

        [HttpGet("{platoonId}")]
        public async Task<ActionResult<List<StudentDto>>> GetStudentsByPlatoon(int platoonId)
        {
            var students = await _context.GetStudentsByPlatoonAsync(platoonId);
            return Ok(students);
        }

        [HttpGet]
        public async Task<ActionResult<List<PlatoonSummaryDto>>> GetAllPlatoons()
        {
            var platoons = await _context.GetAllPlatoonsAsync();
            return Ok(platoons);
        }

        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var exists = await CheckIfStudentExists(id);
            if (!exists)
            {
                return NotFound();
            }

            await _context.DeleteStudentAndUserAsync(id);
            return NoContent();
        }

        private async Task<bool> CheckIfStudentExists(int studentId)
        {
            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM \"Student\" WHERE \"PK_Student\" = @StudentId", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@StudentId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = studentId });
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlatoon(int id)
        {
            // Проверка существования взвода
            var exists = await CheckIfPlatoonExists(id);
            if (!exists)
            {
                return NotFound();
            }

            // Удаление взвода и связанных записей
            await _context.DeletePlatoonAsync(id);

            return NoContent();
        }

        private async Task<bool> CheckIfPlatoonExists(int platoonId)
        {
            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM \"Platoons\" WHERE \"PK_Platoons\" = @PlatoonId", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonId });
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
    }

    public class PlatoonDto
    {
        public string Name { get; set; }
        public bool Status { get; set; }
        public int VisitDayId { get; set; }
        public int DepartmentId { get; set; }
    }
    public class CreateStudentRequestDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool SessionStatus { get; set; }
        public int RoleId { get; set; }
        public int PlatoonsId { get; set; }
        public int SequenceNumber { get; set; }
    }
    public class StudentDto
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public bool SessionStatus { get; set; }
        public int RoleId { get; set; }
        public int SequenceNumber { get; set; }
    }
    public class PlatoonSummaryDto
    {
        public int PlatoonId { get; set; }
        public string Name { get; set; }
    }
}