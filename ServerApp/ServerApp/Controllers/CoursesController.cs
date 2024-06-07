﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            public List<ClassContentDto> ClassContents { get; set; }
            public List<ClassFileDto> ClassFiles { get; set; }
        }

        public class ClassContentDto
        {
            public string ContentType { get; set; }
            public string Content { get; set; }
            public string FilePath { get; set; }
            public string FileData { get; set; }
        }

        public class ClassFileDto
        {
            public string FilePath { get; set; }
            public string FileData { get; set; }
        }
    }
}