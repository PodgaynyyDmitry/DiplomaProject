using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Data;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

                if (!string.IsNullOrEmpty(contentItem.FileData) && IsValidBase64(contentItem.FileData))
                {
                    var (fileData, extension) = ExtractFileDataAndExtension(contentItem.FileData);
                    fileName = $"content_{informationUnitId}_{Guid.NewGuid()}.{extension}";
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

                if (!string.IsNullOrEmpty(file.FileData) && IsValidBase64(file.FileData))
                {
                    var (fileData, extension) = ExtractFileDataAndExtension(file.FileData);
                    fileName = $"file_{informationUnitId}_{Guid.NewGuid()}.{extension}";
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

        [HttpGet("{id}")]
        public async Task<ActionResult<InformationUnitDto>> Get(int id)
        {
            var informationUnit = await _context.GetInformationUnitAsync(id);
            if (informationUnit == null)
            {
                return NotFound();
            }

            var contentItems = await _context.GetContentItemsAsync(id);
            var files = await _context.GetFilesAsync(id);

            informationUnit.ContentItems = contentItems;
            informationUnit.Files = files;

            return Ok(informationUnit);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"InformationUnit\" WHERE \"PK_InformationUnit\" = {0}", id);
            if (exists == 0)
            {
                return NotFound();
            }
            await _context.DeleteInformationUnitAsync(id);
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InformationUnitDto informationUnitDto)
        {
            // Удаляем существующую информационную единицу
            var exists = await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"InformationUnit\" WHERE \"PK_InformationUnit\" = {0}", id);
            if (exists == 0)
            {
                return NotFound();
            }

            await _context.DeleteInformationUnitAsync(id);

            // Создаем новую информационную единицу с новыми данными
            int newInformationUnitId = await _context.CreateInformationUnitAsync(
                informationUnitDto.Title,
                informationUnitDto.AccessModifier,
                informationUnitDto.ChapterId,
                informationUnitDto.CreationDate);

            var baseUrl = $"{Request.Scheme}://{Request.Host}/uploads/";

            foreach (var contentItem in informationUnitDto.ContentItems)
            {
                string filePath = null;
                string fileName = null;

                if (!string.IsNullOrEmpty(contentItem.FileData) && IsValidBase64(contentItem.FileData))
                {
                    var (fileData, extension) = ExtractFileDataAndExtension(contentItem.FileData);
                    fileName = $"content_{newInformationUnitId}_{Guid.NewGuid()}.{extension}";
                    filePath = Path.Combine("Uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                    filePath = Path.Combine(baseUrl, fileName);
                }

                await _context.CreateContentItemAsync(
                    newInformationUnitId,
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

                if (!string.IsNullOrEmpty(file.FileData) && IsValidBase64(file.FileData))
                {
                    var (fileData, extension) = ExtractFileDataAndExtension(file.FileData);
                    fileName = $"file_{newInformationUnitId}_{Guid.NewGuid()}.{extension}";
                    filePath = Path.Combine("Uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    await System.IO.File.WriteAllBytesAsync(filePath, fileData);
                    filePath = Path.Combine(baseUrl, fileName);
                }

                await _context.CreateFileAsync(
                    newInformationUnitId,
                    filePath,
                    informationUnitDto.Files.IndexOf(file) + 1,
                    fileName);
            }

            return Ok(new { id = newInformationUnitId });
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<InformationUnitSummaryDto>>> GetAll()
        {
            var informationUnits = await _context.GetAllInformationUnitsAsync();
            return Ok(informationUnits);
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

    public class InformationUnitSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public ContentItemDto FirstContentItem { get; set; }
    }
}