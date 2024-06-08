using Microsoft.AspNetCore.Mvc;
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

    }

    public class PlatoonScheduleDto
    {
        public int PlatoonsId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
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
    

