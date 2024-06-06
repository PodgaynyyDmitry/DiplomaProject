﻿using ServerApp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerApp.Controllers;
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
        public async Task<InformationUnitDto> GetInformationUnitAsync(int id)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_information_unit(@id)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id });
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new InformationUnitDto
                            {
                                Title = reader.GetString(0),
                                AccessModifier = reader.GetBoolean(1),
                                ChapterId = reader.GetInt32(2),
                                CreationDate = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
            return null;
        }

        public async Task<List<ContentItemDto>> GetContentItemsAsync(int informationUnitId)
        {
            var contentItems = new List<ContentItemDto>();
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_content_items(@informationUnitId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@informationUnitId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = informationUnitId });
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            contentItems.Add(new ContentItemDto
                            {
                                ContentType = reader.GetString(0),
                                Content = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Description = reader.GetString(2),
                                FilePath = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
            return contentItems;
        }

        public async Task<List<FileDto>> GetFilesAsync(int informationUnitId)
        {
            var files = new List<FileDto>();
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_files(@informationUnitId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@informationUnitId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = informationUnitId });
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            files.Add(new FileDto
                            {
                                Path = reader.GetString(0),
                                FileName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
            return files;
        }

        public async Task DeleteInformationUnitAsync(int id)
        {
            var idParam = new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id };
            await this.Database.ExecuteSqlRawAsync("SELECT delete_information_unit(@id)", idParam);
        }
    }
}