using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ServerApp.Data;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.ValidateUserAsync(loginDto.Login, loginDto.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, loginDto.Login),
            new Claim(ClaimTypes.NameIdentifier, user.PK_Users.ToString()),
            new Claim(ClaimTypes.Role, user.PK_Role.ToString())
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok("Login successful");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logout successful");
        }

        [HttpGet("accessdenied")]
        public IActionResult AccessDenied()
        {
            return Forbid();
        }
    }
}

    public class LoginDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class UserRightDto
    {
        public string RightName { get; set; }
        public bool Writing { get; set; }
        public bool Reading { get; set; }
    }

