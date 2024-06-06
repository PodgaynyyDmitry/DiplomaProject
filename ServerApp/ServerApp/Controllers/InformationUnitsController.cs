using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Data;
using Newtonsoft.Json;

namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/informationunits")]
    public class InformationUnitsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InformationUnitsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] InformationUnitDto informationUnitDto)
        {
            int informationUnitId = await _context.CreateInformationUnitAsync(
                informationUnitDto.Title,
                informationUnitDto.AccessModifier,
                informationUnitDto.ChapterId,
                informationUnitDto.CreationDate);

            var baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";

            foreach (var contentItem in informationUnitDto.ContentItems)
            {
                string filePath = null;
                string fileName = null;

                if (!string.IsNullOrEmpty(contentItem.FileData) && IsValidBase64(contentItem.FileData.Split(',')[1]))
                {
                    var fileData = Convert.FromBase64String(contentItem.FileData.Split(',')[1]);
                    fileName = $"content_{informationUnitId}_{Guid.NewGuid()}";
                    filePath = Path.Combine("Uploads", fileName); // Обновление пути для сохранения файлов
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Создание папки, если она не существует
                    await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                    filePath = Path.Combine(baseUrl, fileName);
                }

                await _context.CreateContentItemAsync(
                    informationUnitId,
                    informationUnitDto.ContentItems.IndexOf(contentItem) + 1,
                    contentItem.ContentType,
                    contentItem.Content,
                    filePath,
                    contentItem.Description);
            }

            foreach (var file in informationUnitDto.Files)
            {
                string filePath = null;
                string fileName = null;

                if (!string.IsNullOrEmpty(file.FileData) && IsValidBase64(file.FileData.Split(',')[1]))
                {
                    var fileData = Convert.FromBase64String(file.FileData.Split(',')[1]);
                    fileName = $"file_{informationUnitId}_{Guid.NewGuid()}";
                    filePath = Path.Combine("Uploads", fileName); // Обновление пути для сохранения файлов
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Создание папки, если она не существует
                    await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                    filePath = Path.Combine(baseUrl, fileName);
                }

                await _context.CreateFileAsync(
                    informationUnitId,
                    filePath,
                    informationUnitDto.Files.IndexOf(file) + 1,
                    fileName);
            }

            return CreatedAtAction(nameof(Create), new { id = informationUnitId }, informationUnitDto);
        }

        private bool IsValidBase64(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }

    public class InformationUnitDto
    {
        public string Title { get; set; }
        public bool AccessModifier { get; set; }
        public int ChapterId { get; set; }
        public DateTime CreationDate { get; set; }
        public List<ContentItemDto> ContentItems { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class ContentItemDto
    {
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string FileData { get; set; }
    }

    public class FileDto
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
    }
}