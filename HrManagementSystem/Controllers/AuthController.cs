using HrManagementSystem.DTOs.AuthDTOs;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EndpointSummary("Login for all users (Admin, HR, Employee)")]
        public async Task<IActionResult> Login([FromForm] LoginDTO loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register-hr")]
        [Authorize(Roles = "Admin")]
        [EndpointSummary("Register HR (Admin only)")]
        public async Task<IActionResult> RegisterHR([FromForm] AddEmployee registerDto)
        {
            
            try
            {
                var response = await _authService.RegisterHRAsync(registerDto);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        [EndpointSummary("Get current user profile")]
        public IActionResult GetProfile()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            return Ok(new { email, role, fullName });
        }
    }
}
