using Microsoft.AspNetCore.Mvc;
using TaskManager3.Models;
using TaskManager3.Services;

namespace TaskManager3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var token = await _authService.RegisterAsync(user.Email, user.PasswordHash);
            if (token == null)
            {
                return Unauthorized("User registration failed");
            }
            return Ok(new { token });
        }

        // Login an existing user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var token = await _authService.LoginAsync(user.Email, user.PasswordHash);
            if (token == null)
            {
                return Unauthorized("Invalid credentials");
            }
            return Ok(new { token });
        }
    }
}
