using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ServerApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<InformationUnit> InformationUnits { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<Models.File> Files { get; set; }

        public async Task<int> CreateInformationUnitAsync(string title, bool accessModifier, int chapterId, DateTime creationDate)
        {
            var titleParam = new NpgsqlParameter("@Title", title);
            var accessModifierParam = new NpgsqlParameter("@AccessModifier", accessModifier);
            var chapterIdParam = new NpgsqlParameter("@ChapterId", chapterId);
            var creationDateParam = new NpgsqlParameter("@CreationDate", creationDate);

            var result = await Database.ExecuteSqlRawAsync(
                "INSERT INTO \"InformationUnit\" (\"Title\", \"AccessModifier\", \"PK_Chapter\", \"CreationDate\") VALUES (@Title, @AccessModifier, @ChapterId, @CreationDate)",
                titleParam, accessModifierParam, chapterIdParam, creationDateParam);

            return result;
        }

        public async Task CreateContentItemAsync(int informationUnitId, int sequenceNumber, string contentType, string content, string filePath)
        {
            var informationUnitIdParam = new NpgsqlParameter("@p_information_unit_id", informationUnitId);
            var sequenceNumberParam = new NpgsqlParameter("@p_sequence_number", sequenceNumber);
            var contentTypeParam = new NpgsqlParameter("@p_content_type", contentType);
            var contentParam = new NpgsqlParameter("@p_content", content ?? (object)DBNull.Value);
            var filePathParam = new NpgsqlParameter("@p_file_path", filePath ?? (object)DBNull.Value);

            await Database.ExecuteSqlRawAsync(
                "SELECT create_content_item(@p_information_unit_id, @p_sequence_number, @p_content_type, @p_content, @p_file_path)",
                informationUnitIdParam, sequenceNumberParam, contentTypeParam, contentParam, filePathParam);
        }
        public async Task CreateFileAsync(int informationUnitId, string path, int sequenceNumber)
        {
            var informationUnitIdParam = new NpgsqlParameter("@InformationUnitId", informationUnitId);
            var pathParam = new NpgsqlParameter("@Path", path);
            var sequenceNumberParam = new NpgsqlParameter("@SequenceNumber", sequenceNumber);

            await Database.ExecuteSqlRawAsync(
                "INSERT INTO \"File\" (\"InformationUnitId\", \"Path\", \"SequenceNumber\") VALUES (@InformationUnitId, @Path, @SequenceNumber)",
                informationUnitIdParam, pathParam, sequenceNumberParam);
        }
    }
}
