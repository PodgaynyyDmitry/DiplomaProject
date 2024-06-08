using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Data;
using ServerApp.Models;
using System.Threading.Tasks;

namespace ServerApp.Controllers
{
    namespace ServerApp.Controllers
    {
        [ApiController]
        [Route("api/courses")]
        public class CoursesController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly IWebHostEnvironment _env;

            public CoursesController(AppDbContext context, IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }

            [HttpPost("discipline")]
            public async Task<ActionResult> CreateDiscipline([FromBody] DisciplineDto disciplineDto)
            {
                int disciplineId = await _context.CreateDisciplineAsync(
                    disciplineDto.Module,
                    disciplineDto.Chapter,
                    disciplineDto.DisciplineNumber,
                    disciplineDto.Title,
                    disciplineDto.DepartmentId);

                if (disciplineId <= 0)
                {
                    return BadRequest("Discipline creation failed.");
                }

                return CreatedAtAction(nameof(CreateDiscipline), new { id = disciplineId }, disciplineDto);
            }

            [HttpPost("class")]
            public async Task<ActionResult> CreateClass([FromBody] ClassDto classDto)
            {
                int classId = await _context.CreateClassAsync(
                    classDto.DisciplineId,
                    classDto.StartDate,
                    classDto.Duration,
                    classDto.Topic,
                    classDto.ClassTypeId,
                    classDto.TeacherId,
                    classDto.UserId,
                    classDto.ClassRoomId,
                    classDto.PlatoonsId);

                if (classId <= 0)
                {
                    return BadRequest("Class creation failed.");
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";

                foreach (var contentItem in classDto.ClassContents)
                {
                    string filePath = null;
                    string fileName = null;

                    if (!string.IsNullOrEmpty(contentItem.FileData) && IsValidBase64(contentItem.FileData))
                    {
                        var (fileData, extension) = ExtractFileDataAndExtension(contentItem.FileData);
                        fileName = $"class_content_{classId}_{Guid.NewGuid()}.{extension}";
                        filePath = Path.Combine("Uploads", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                        filePath = Path.Combine(baseUrl, fileName);
                    }

                    await _context.CreateClassContentAsync(
                        classId,
                        classDto.ClassContents.IndexOf(contentItem) + 1,
                        contentItem.ContentType,
                        contentItem.Content,
                        filePath);
                }

                foreach (var file in classDto.ClassFiles)
                {
                    string filePath = null;
                    string fileName = null;

                    if (!string.IsNullOrEmpty(file.FileData) && IsValidBase64(file.FileData))
                    {
                        var (fileData, extension) = ExtractFileDataAndExtension(file.FileData);
                        fileName = $"class_file_{classId}_{Guid.NewGuid()}.{extension}";
                        filePath = Path.Combine("Uploads", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                        filePath = Path.Combine(baseUrl, fileName);
                    }

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _context.CreateClassFileAsync(
                            classId,
                            classDto.ClassFiles.IndexOf(file) + 1,
                            filePath);
                    }
                }

                return CreatedAtAction(nameof(CreateClass), new { id = classId }, classDto);
            }

            [HttpGet("class/{id}")]
            public async Task<ActionResult<ClassDto>> GetClass(int id)
            {
                var classInfo = await _context.GetClassAsync(id);
                if (classInfo == null)
                {
                    return NotFound();
                }

                var classContent = await _context.GetClassContentAsync(id);
                var classFiles = await _context.GetClassFilesAsync(id);

                classInfo.ClassContents = classContent;
                classInfo.ClassFiles = classFiles;

                return Ok(classInfo);
            }

            [HttpDelete("class/{id}")]
            public async Task<IActionResult> DeleteClass(int id)
            {
                var exists = await CheckIfClassExists(id);
                if (!exists)
                {
                    return NotFound();
                }

                await _context.DeleteClassAsync(id);
                return NoContent();
            }

            [HttpPut("class/{id}")]
            public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassDto classDto)
            {
                var exists = await CheckIfClassExists(id);
                if (!exists)
                {
                    return NotFound();
                }

                int newClassId = await _context.UpdateClassAsync(id, classDto);
                return Ok(new { id = newClassId });
            }

            [HttpGet("disciplines")]
            public async Task<ActionResult<IEnumerable<DisciplineSummaryDto>>> GetAllDisciplines()
            {
                var disciplines = await _context.GetAllDisciplinesAsync();
                return Ok(disciplines);
            }

            [HttpGet("classes/{disciplineId}")]
            public async Task<ActionResult<IEnumerable<ClassSummaryDto>>> GetAllClassesByDiscipline(int disciplineId)
            {
                var classes = await _context.GetAllClassesByDisciplineAsync(disciplineId);
                return Ok(classes);
            }



            private async Task<bool> CheckIfClassExists(int classId)
            {
                var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync();
                try
                {
                    using (var cmd = new NpgsqlCommand("SELECT 1 FROM \"Class\" WHERE \"PK_Class\" = @ClassId", conn))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("@ClassId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classId });
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null && result != DBNull.Value;
                    }
                }
                finally
                {
                    await conn.CloseAsync();
                }
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

        public class DisciplineDto
        {
            public string Module { get; set; }
            public string Chapter { get; set; }
            public string DisciplineNumber { get; set; }
            public string Title { get; set; }
            public int DepartmentId { get; set; }
        }

        public class ClassDto
        {
            public int DisciplineId { get; set; }
            public DateTime StartDate { get; set; }
            public int Duration { get; set; }
            public string Topic { get; set; }
            public int ClassTypeId { get; set; }
            public int TeacherId { get; set; }
            public int UserId { get; set; }
            public int ClassRoomId { get; set; }
            public int PlatoonsId { get; set; }
            public List<ClassContentDto> ClassContents { get; set; } = new List<ClassContentDto>();
            public List<ClassFileDto> ClassFiles { get; set; } = new List<ClassFileDto>();
        }

        public class ClassContentDto
        {
            public int SequenceNumber { get; set; }
            public string ContentType { get; set; }
            public string Content { get; set; }
            public string FilePath { get; set; }
            public string FileData { get; set; } // Добавляем это свойство
        }

        public class ClassFileDto
        {
            public int SequenceNumber { get; set; }
            public string FilePath { get; set; }
            public string FileData { get; set; } // Добавляем это свойство
        }
        public class DisciplineSummaryDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }
        public class ClassSummaryDto
        {
            public int Id { get; set; }
            public string Topic { get; set; }
        }

    }
}