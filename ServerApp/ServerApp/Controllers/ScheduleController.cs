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
    [Route("api/schedule")]
    public class ScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScheduleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePlatoonSchedule([FromBody] PlatoonScheduleDto platoonScheduleDto)
        {
            if (platoonScheduleDto == null)
            {
                return BadRequest("Platoon schedule data is required.");
            }

            int platoonScheduleId = await _context.CreatePlatoonScheduleAsync(
                platoonScheduleDto.PlatoonsId,
                platoonScheduleDto.WeekStartDate,
                platoonScheduleDto.WeekEndDate);

            return CreatedAtAction(nameof(CreatePlatoonSchedule), new { id = platoonScheduleId }, platoonScheduleDto);
        }

        [HttpPost("academic-hour")]
        public async Task<ActionResult> CreateAcademicHour([FromBody] AcademicHourDto academicHourDto)
        {
            int academicHourId = await _context.CreateAcademicHourAsync(
                academicHourDto.PlatoonScheduleId,
                academicHourDto.PlatoonsId,
                academicHourDto.SequenceNumber,
                academicHourDto.NumberOfHours,
                academicHourDto.DisciplineCode,
                academicHourDto.Topic,
                academicHourDto.ClassTypeId);

            return CreatedAtAction(nameof(CreateAcademicHour), new { id = academicHourId }, academicHourDto);
        }

        [HttpGet("academic-hours/{platoonScheduleId}")]
        public async Task<ActionResult<IEnumerable<AcademicHourDto>>> GetAcademicHours(int platoonScheduleId)
        {
            var academicHours = await _context.GetAcademicHoursAsync(platoonScheduleId);
            return Ok(academicHours);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlatoonSchedule(int id)
        {
            var commandText = "SELECT COUNT(*) FROM \"PlatoonSchedule\" WHERE \"PK_PlatoonSchedule\" = @id";
            var parameter = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id };

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(parameter);
                _context.Database.OpenConnection();

                var result = await command.ExecuteScalarAsync();
                _context.Database.CloseConnection();

                if (Convert.ToInt32(result) == 0)
                {
                    return NotFound();
                }
            }

            await _context.DeletePlatoonScheduleAsync(id);
            return NoContent();
        }

        [HttpDelete("academic-hour/{id}")]
        public async Task<IActionResult> DeleteAcademicHour(int id)
        {
            var commandText = "SELECT COUNT(*) FROM \"AcademicHour\" WHERE \"PK_AcademicHour\" = @id";
            var parameter = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id };

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(parameter);
                _context.Database.OpenConnection();

                var result = await command.ExecuteScalarAsync();
                _context.Database.CloseConnection();

                if (Convert.ToInt32(result) == 0)
                {
                    return NotFound();
                }
            }

            await _context.DeleteAcademicHourAsync(id);
            return NoContent();
        }


        [HttpPut("academic-hour/{id}")]
        public async Task<ActionResult> UpdateAcademicHour(int id, [FromBody] AcademicHourDto academicHourDto)
        {
            // Проверка существования записи
            var exists = await _context.CheckIfAcademicHourExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            // Обновление записи
            await _context.UpdateAcademicHourAsync(
                id,
                academicHourDto.PlatoonScheduleId,
                academicHourDto.PlatoonsId,
                academicHourDto.SequenceNumber,
                academicHourDto.NumberOfHours,
                academicHourDto.DisciplineCode,
                academicHourDto.Topic,
                academicHourDto.ClassTypeId
            );

            return NoContent();
        }
    }

    public class PlatoonScheduleDto
    {
        public int PlatoonsId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
       // public List<AcademicHourDto> AcademicHours { get; set; }
    }

    public class AcademicHourDto
    {
        public int AcademicHourId { get; set; }
        public int PlatoonScheduleId { get; set; }
        public int PlatoonsId { get; set; }
        public int SequenceNumber { get; set; }
        public int NumberOfHours { get; set; }
        public string DisciplineCode { get; set; }
        public string Topic { get; set; }
        public int ClassTypeId { get; set; }
    }
    public class AcademicHourRequestDto
    {
        public int PlatoonScheduleId { get; set; }
    }
}
    

