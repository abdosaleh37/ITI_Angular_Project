using ITI_Project.BLL.DTOs;
using ITI_Project.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITI_Project.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class ApplicationUserController : ControllerBase
    {
        private readonly IAuthService _authService;

        public ApplicationUserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var registerDto = new RegisterUserDto
            {
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.UserName,
                Password = dto.Password,
                ConfirmPassword = dto.ConfirmPassword
            };

            var result = await _authService.RegisterAsync(registerDto);

            if (result.IsFailure)
            {
                // Handle multiple errors
                if (result.Errors.Count > 1)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return BadRequest(ModelState);
                }

                // Handle single error with appropriate status code
                if (result.Error!.Contains("already exists"))
                {
                    return Conflict(new { Message = result.Error });
                }

                return BadRequest(new { Message = result.Error });
            }

            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto.Email, dto.Password);

            if (result.IsFailure)
            {
                return Unauthorized(new { Message = result.Error });
            }

            return Ok(new
            {
                result.Value!.Token,
                result.Value.Expiration,
                result.Value.User
            });
        }
    }
}
