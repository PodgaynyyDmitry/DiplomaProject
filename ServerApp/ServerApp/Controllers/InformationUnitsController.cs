using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Data;

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
            // Создаем информационную единицу
            int informationUnitId = await _context.CreateInformationUnitAsync(
                informationUnitDto.Title,
                informationUnitDto.AccessModifier,
                informationUnitDto.ChapterId,
                informationUnitDto.CreationDate);

            // Создаем контент-элементы
            // Создаем контент-элементы
            for (int i = 0; i < informationUnitDto.ContentItems.Count; i++)
            {
                var contentItem = informationUnitDto.ContentItems[i];
                await _context.CreateContentItemAsync(
                    informationUnitId,
                    i + 1,
                    contentItem.ContentType,
                    contentItem.Content,
                    contentItem.FilePath);
            }

            // Создаем файлы
            for (int i = 0; i < informationUnitDto.Files.Count; i++)
            {
                var file = informationUnitDto.Files[i];
                await _context.CreateFileAsync(
                    informationUnitId,
                    file.Path,
                    i + 1);
            }

            return CreatedAtAction(nameof(Create), new { id = informationUnitId }, informationUnitDto);
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
    }

    public class FileDto
    {
        public string Path { get; set; }
    }
}
