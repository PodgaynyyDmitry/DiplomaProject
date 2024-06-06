using ServerApp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ServerApp.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<InformationUnit> InformationUnits { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<Models.File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InformationUnit>().HasKey(iu => iu.PK_InformationUnit);
            modelBuilder.Entity<ContentItem>().HasKey(ci => ci.PK_ContentItem);
            modelBuilder.Entity<Models.File>().HasKey(f => f.PK_File);
        }

        public async Task<int> CreateInformationUnitAsync(string title, bool accessModifier, int chapterId, DateTime creationDate)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_information_unit(@Title, @AccessModifier, @ChapterId, @CreationDate)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@Title", NpgsqlTypes.NpgsqlDbType.Text) { Value = title });
                    cmd.Parameters.Add(new NpgsqlParameter("@AccessModifier", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = accessModifier });
                    cmd.Parameters.Add(new NpgsqlParameter("@ChapterId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = chapterId });
                    cmd.Parameters.Add(new NpgsqlParameter("@CreationDate", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = creationDate });

                    var result = await cmd.ExecuteScalarAsync();
                    return (int)result;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task CreateContentItemAsync(int informationUnitId, int sequenceNumber, string contentType, string content, string filePath, string description)
        {
            var informationUnitIdParam = new NpgsqlParameter("@InformationUnitId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = informationUnitId };
            var sequenceNumberParam = new NpgsqlParameter("@SequenceNumber", NpgsqlTypes.NpgsqlDbType.Integer) { Value = sequenceNumber };
            var contentTypeParam = new NpgsqlParameter("@ContentType", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = contentType };
            var contentParam = new NpgsqlParameter("@Content", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)content ?? DBNull.Value };
            var filePathParam = new NpgsqlParameter("@FilePath", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)filePath ?? DBNull.Value };
            var descriptionParam = new NpgsqlParameter("@Description", NpgsqlTypes.NpgsqlDbType.Text) { Value = description };

            await this.Database.ExecuteSqlRawAsync(
                "SELECT create_content_item(@InformationUnitId, @SequenceNumber, @ContentType, @Content, @FilePath, @Description)",
                informationUnitIdParam, sequenceNumberParam, contentTypeParam, contentParam, filePathParam, descriptionParam);
        }

        public async Task CreateFileAsync(int informationUnitId, string path, int sequenceNumber, string fileName)
        {
            var informationUnitIdParam = new NpgsqlParameter("@InformationUnitId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = informationUnitId };
            var pathParam = new NpgsqlParameter("@Path", NpgsqlTypes.NpgsqlDbType.Text) { Value = path };
            var sequenceNumberParam = new NpgsqlParameter("@SequenceNumber", NpgsqlTypes.NpgsqlDbType.Integer) { Value = sequenceNumber };
            var fileNameParam = new NpgsqlParameter("@FileName", NpgsqlTypes.NpgsqlDbType.Text) { Value = fileName };

            await this.Database.ExecuteSqlRawAsync(
                "SELECT create_file(@InformationUnitId, @Path, @SequenceNumber, @FileName)",
                informationUnitIdParam, pathParam, sequenceNumberParam, fileNameParam);
        }
    }
}