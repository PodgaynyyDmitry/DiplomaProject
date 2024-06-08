using ServerApp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerApp.Controllers;
using System.Security.Claims;
using ServerApp.Controllers.ServerApp.Controllers;
namespace ServerApp.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<InformationUnit> InformationUnits { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<Models.File> Files { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassContent> ClassContents { get; set; }
        public DbSet<ClassFile> ClassFiles { get; set; }

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
        public async Task<List<InformationUnitSummaryDto>> GetAllInformationUnitsAsync()
        {
            var informationUnits = new List<InformationUnitSummaryDto>();

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT \"PK_InformationUnit\", \"Title\", \"CreationDate\" FROM \"InformationUnit\" ORDER BY \"PK_InformationUnit\" DESC";
                this.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var unit = new InformationUnitSummaryDto
                        {
                            Id = result.GetInt32(0),
                            Title = result.GetString(1),
                            CreationDate = result.GetDateTime(2)
                        };
                        informationUnits.Add(unit);
                    }
                }
            }

            foreach (var unit in informationUnits)
            {
                var contentItem = await GetFirstContentItemAsync(unit.Id);
                unit.FirstContentItem = contentItem;
            }

            return informationUnits;
        }

        private async Task<ContentItemDto> GetFirstContentItemAsync(int informationUnitId)
        {
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT \"ContentType\", \"Content\", \"FilePath\", \"Description\" FROM \"ContentItem\" WHERE \"PK_InformationUnit\" = @informationUnitId ORDER BY \"SequenceNumber\" LIMIT 1";
                command.Parameters.Add(new NpgsqlParameter("@informationUnitId", informationUnitId));
                this.Database.OpenConnection();

                using (var result = await command.ExecuteReaderAsync())
                {
                    if (await result.ReadAsync())
                    {
                        return new ContentItemDto
                        {
                            ContentType = result.GetString(0),
                            Content = result.IsDBNull(1) ? null : result.GetString(1),
                            FilePath = result.IsDBNull(2) ? null : result.GetString(2),
                            Description = result.IsDBNull(3) ? null : result.GetString(3)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<int> CreateDisciplineAsync(string module, string chapter, string disciplineNumber, string title, int departmentId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_discipline(@Module, @Chapter, @DisciplineNumber, @Title, @DepartmentId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@Module", NpgsqlTypes.NpgsqlDbType.Text) { Value = module });
                    cmd.Parameters.Add(new NpgsqlParameter("@Chapter", NpgsqlTypes.NpgsqlDbType.Text) { Value = chapter });
                    cmd.Parameters.Add(new NpgsqlParameter("@DisciplineNumber", NpgsqlTypes.NpgsqlDbType.Text) { Value = disciplineNumber });
                    cmd.Parameters.Add(new NpgsqlParameter("@Title", NpgsqlTypes.NpgsqlDbType.Text) { Value = title });
                    cmd.Parameters.Add(new NpgsqlParameter("@DepartmentId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = departmentId });

                    var result = await cmd.ExecuteScalarAsync();
                    return (int)result;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<int> CreateClassAsync(int disciplineId, DateTime startDate, int duration, string topic, int classTypeId, int teacherId, int userId, int classRoomId, int platoonsId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_class(@DisciplineId, @StartDate, @Duration, @Topic, @ClassTypeId, @TeacherId, @UserId, @ClassRoomId, @PlatoonsId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@DisciplineId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = disciplineId });
                    cmd.Parameters.Add(new NpgsqlParameter("@StartDate", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = startDate });
                    cmd.Parameters.Add(new NpgsqlParameter("@Duration", NpgsqlTypes.NpgsqlDbType.Integer) { Value = duration });
                    cmd.Parameters.Add(new NpgsqlParameter("@Topic", NpgsqlTypes.NpgsqlDbType.Text) { Value = topic });
                    cmd.Parameters.Add(new NpgsqlParameter("@ClassTypeId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classTypeId });
                    cmd.Parameters.Add(new NpgsqlParameter("@TeacherId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = teacherId });
                    cmd.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = userId });
                    cmd.Parameters.Add(new NpgsqlParameter("@ClassRoomId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classRoomId });
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonsId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonsId });

                    var result = await cmd.ExecuteScalarAsync();
                    return (int)result;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task CreateClassContentAsync(int classId, int sequenceNumber, string contentType, string content, string filePath)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_class_content(@ClassId, @SequenceNumber, @ContentType, @Content, @FilePath)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@ClassId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classId });
                    cmd.Parameters.Add(new NpgsqlParameter("@SequenceNumber", NpgsqlTypes.NpgsqlDbType.Integer) { Value = sequenceNumber });
                    cmd.Parameters.Add(new NpgsqlParameter("@ContentType", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = contentType });
                    cmd.Parameters.Add(new NpgsqlParameter("@Content", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)content ?? DBNull.Value });
                    cmd.Parameters.Add(new NpgsqlParameter("@FilePath", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)filePath ?? DBNull.Value });

                    await cmd.ExecuteScalarAsync();
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task CreateClassFileAsync(int classId, int sequenceNumber, string filePath)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_class_file(@ClassId, @SequenceNumber, @FilePath)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@ClassId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classId });
                    cmd.Parameters.Add(new NpgsqlParameter("@SequenceNumber", NpgsqlTypes.NpgsqlDbType.Integer) { Value = sequenceNumber });
                    cmd.Parameters.Add(new NpgsqlParameter("@FilePath", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object)filePath ?? DBNull.Value });

                    await cmd.ExecuteScalarAsync();
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<ClassDto> GetClassAsync(int id)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_class(@id)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id });
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ClassDto
                            {
                                DisciplineId = reader.GetInt32(0),
                                StartDate = reader.GetDateTime(1),
                                Duration = reader.GetInt32(2),
                                Topic = reader.GetString(3),
                                ClassTypeId = reader.GetInt32(4),
                                TeacherId = reader.GetInt32(5),
                                UserId = reader.GetInt32(6),
                                ClassRoomId = reader.GetInt32(7),
                                PlatoonsId = reader.GetInt32(8)
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

        public async Task<List<ClassContentDto>> GetClassContentAsync(int classId)
        {
            var classContents = new List<ClassContentDto>();
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_class_content(@classId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@classId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classId });
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            classContents.Add(new ClassContentDto
                            {
                                SequenceNumber = reader.GetInt32(0),
                                ContentType = reader.GetString(1),
                                Content = reader.IsDBNull(2) ? null : reader.GetString(2),
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
            return classContents;
        }

        public async Task<List<ClassFileDto>> GetClassFilesAsync(int classId)
        {
            var classFiles = new List<ClassFileDto>();
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_class_files(@classId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@classId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classId });
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            classFiles.Add(new ClassFileDto
                            {
                                SequenceNumber = reader.GetInt32(0),
                                FilePath = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
            return classFiles;
        }

        public async Task DeleteClassAsync(int classId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT delete_class(@classId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@classId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classId });
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<int> UpdateClassAsync(int classId, ClassDto classDto)
        {
            await DeleteClassAsync(classId);
            int newClassId = await CreateClassAsync(
                classDto.DisciplineId,
                classDto.StartDate,
                classDto.Duration,
                classDto.Topic,
                classDto.ClassTypeId,
                classDto.TeacherId,
                classDto.UserId,
                classDto.ClassRoomId,
                classDto.PlatoonsId
            );

            foreach (var contentItem in classDto.ClassContents)
            {
                await CreateClassContentAsync(
                    newClassId,
                    contentItem.SequenceNumber,
                    contentItem.ContentType,
                    contentItem.Content,
                    contentItem.FilePath
                );
            }

            foreach (var file in classDto.ClassFiles)
            {
                await CreateClassFileAsync(
                    newClassId,
                    file.SequenceNumber,
                    file.FilePath
                );
            }

            return newClassId;
        }

        public async Task<List<DisciplineSummaryDto>> GetAllDisciplinesAsync()
        {
            var disciplines = new List<DisciplineSummaryDto>();

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT \"PK_Discipline\", \"Title\" FROM \"Discipline\" ORDER BY \"PK_Discipline\" DESC";
                this.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var discipline = new DisciplineSummaryDto
                        {
                            Id = result.GetInt32(0),
                            Title = result.GetString(1)
                        };
                        disciplines.Add(discipline);
                    }
                }
            }

            return disciplines;
        }

        public async Task<List<ClassSummaryDto>> GetAllClassesByDisciplineAsync(int disciplineId)
        {
            var classes = new List<ClassSummaryDto>();

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT \"PK_Class\", \"Topic\" FROM \"Class\" WHERE \"PK_Discipline\" = @DisciplineId ORDER BY \"PK_Class\" DESC";
                command.Parameters.Add(new NpgsqlParameter("@DisciplineId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = disciplineId });
                this.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var classItem = new ClassSummaryDto
                        {
                            Id = result.GetInt32(0),
                            Topic = result.GetString(1)
                        };
                        classes.Add(classItem);
                    }
                }
            }

            return classes;
        }

        public async Task<int> CreatePlatoonScheduleAsync(int platoonsId, DateTime weekStartDate, DateTime weekEndDate)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_platoon_schedule(@PlatoonsId, @WeekStartDate, @WeekEndDate)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonsId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonsId });
                    cmd.Parameters.Add(new NpgsqlParameter("@WeekStartDate", NpgsqlTypes.NpgsqlDbType.Date) { Value = weekStartDate });
                    cmd.Parameters.Add(new NpgsqlParameter("@WeekEndDate", NpgsqlTypes.NpgsqlDbType.Date) { Value = weekEndDate });

                    var result = await cmd.ExecuteScalarAsync();
                    return (int)result;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<int> CreateAcademicHourAsync(int platoonScheduleId,int platoonsId, int sequenceNumber,int numberOfHours, string disciplineCode,string topic,int classTypeId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT create_academic_hour(@PlatoonScheduleId, @PlatoonsId, @SequenceNumber, @NumberOfHours, @DisciplineCode, @Topic, @ClassTypeId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonScheduleId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonScheduleId });
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonsId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonsId });
                    cmd.Parameters.Add(new NpgsqlParameter("@SequenceNumber", NpgsqlTypes.NpgsqlDbType.Integer) { Value = sequenceNumber });
                    cmd.Parameters.Add(new NpgsqlParameter("@NumberOfHours", NpgsqlTypes.NpgsqlDbType.Integer) { Value = numberOfHours });
                    cmd.Parameters.Add(new NpgsqlParameter("@DisciplineCode", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = disciplineCode });
                    cmd.Parameters.Add(new NpgsqlParameter("@Topic", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = topic });
                    cmd.Parameters.Add(new NpgsqlParameter("@ClassTypeId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classTypeId });

                    var result = await cmd.ExecuteScalarAsync();
                    return (int)result;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<List<AcademicHourDto>> GetAcademicHoursAsync(int platoonScheduleId)
        {
            var academicHours = new List<AcademicHourDto>();

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"SELECT ""PK_AcademicHour"", ""PK_PlatoonSchedule"", ""PK_Platoons"", 
                                ""SequenceNumber"", ""NumberOfHours"", ""DisciplineCode"", ""Topic"", ""PK_ClassType"" 
                                FROM ""AcademicHour"" 
                                WHERE ""PK_PlatoonSchedule"" = @PlatoonScheduleId";

                command.Parameters.Add(new NpgsqlParameter("@PlatoonScheduleId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonScheduleId });

                this.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        academicHours.Add(new AcademicHourDto
                        {
                            AcademicHourId = result.GetInt32(0),
                            PlatoonScheduleId = result.GetInt32(1),
                            PlatoonsId = result.GetInt32(2),
                            SequenceNumber = result.GetInt32(3),
                            NumberOfHours = result.GetInt32(4),
                            DisciplineCode = result.IsDBNull(5) ? null : result.GetString(5),
                            Topic = result.GetString(6),
                            ClassTypeId = result.GetInt32(7)
                        });
                    }
                }
            }

            return academicHours;
        }

        public async Task DeletePlatoonScheduleAsync(int id)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT delete_platoon_schedule(@PlatoonScheduleId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonScheduleId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id });
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task DeleteAcademicHourAsync(int academicHourId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT delete_academic_hour(@AcademicHourId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@AcademicHourId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = academicHourId });
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task UpdateAcademicHourAsync(int academicHourId, int platoonScheduleId, int platoonsId, int sequenceNumber, int numberOfHours, string disciplineCode, string topic, int classTypeId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT update_academic_hour(@AcademicHourId, @PlatoonScheduleId, @PlatoonsId, @SequenceNumber, @NumberOfHours, @DisciplineCode, @Topic, @ClassTypeId)", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@AcademicHourId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = academicHourId });
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonScheduleId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonScheduleId });
                    cmd.Parameters.Add(new NpgsqlParameter("@PlatoonsId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = platoonsId });
                    cmd.Parameters.Add(new NpgsqlParameter("@SequenceNumber", NpgsqlTypes.NpgsqlDbType.Integer) { Value = sequenceNumber });
                    cmd.Parameters.Add(new NpgsqlParameter("@NumberOfHours", NpgsqlTypes.NpgsqlDbType.Integer) { Value = numberOfHours });
                    cmd.Parameters.Add(new NpgsqlParameter("@DisciplineCode", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = (object)disciplineCode ?? DBNull.Value });
                    cmd.Parameters.Add(new NpgsqlParameter("@Topic", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = (object)topic ?? DBNull.Value });
                    cmd.Parameters.Add(new NpgsqlParameter("@ClassTypeId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = classTypeId });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<bool> CheckIfAcademicHourExistsAsync(int academicHourId)
        {
            var conn = (NpgsqlConnection)this.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM \"AcademicHour\" WHERE \"PK_AcademicHour\" = @AcademicHourId", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@AcademicHourId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = academicHourId });
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

    }
}

