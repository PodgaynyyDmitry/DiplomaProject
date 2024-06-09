using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServerApp.Data;
using ServerApp.Models;
using System;
using System.Threading.Tasks;


namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/rights")]
    public class RightsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RightsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateCurrentRights([FromBody] List<CreateCurrentRightsRequestDto> rights)
        {
            await _context.CreateCurrentRightsAsync(rights);

            return CreatedAtAction(nameof(CreateCurrentRights), new { count = rights.Count });
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<CurrentRightsDto>>> GetCurrentRights(int userId)
        {
            var rights = await _context.GetCurrentRightsAsync(userId);
            if (rights == null || rights.Count == 0)
            {
                return NotFound();
            }
            return Ok(rights);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserRights(int userId, [FromBody] List<RightDto> rights)
        {
            // Проверка существования пользователя
            var exists = await CheckIfUserExistsAsync(userId);
            if (!exists)
            {
                return NotFound();
            }

            // Обновление прав пользователя
            foreach (var right in rights)
            {
                await _context.UpdateUserRightAsync(userId, right);
            }

            return NoContent();
        }

        private async Task<bool> CheckIfUserExistsAsync(int userId)
        {
            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            await conn.OpenAsync();
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM \"Users\" WHERE \"PK_User\" = @UserId", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = userId });
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
    }

    public class CreateCurrentRightsRequestDto
    {
        public int UserId { get; set; }
        public int RightsId { get; set; }
        public bool Writing { get; set; }
        public bool Reading { get; set; }
    }
    public class CurrentRightsDto
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }
        public string RightsName { get; set; }
        public bool Writing { get; set; }
        public bool Reading { get; set; }
    }
    public class RightDto
    {
        public int RightId { get; set; }
        public bool Writing { get; set; }
        public bool Reading { get; set; }
    }
}