using ITI_Project.API.Contracts;
using ITI_Project.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ITI_Project.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class ApplicationUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(dto.Password != dto.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password and Confirm Password do not match.");
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser is not null)
            {
                return Conflict(new { Message = "Email already exists" });
            }

            existingUser = await _userManager.FindByNameAsync(dto.UserName);

            if (existingUser is not null)
            {
                return Conflict(new { Message = "UserName already exists" });
            }

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.Name
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User registered successfully" });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if(user is null || string.IsNullOrEmpty(user.UserName))
            {
                return Unauthorized(new { Message = "Invalid Email or Password" });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, dto.Password, false, false);

            if(!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid Email or Password" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? String.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? String.Empty),
                new Claim("FullName", user.FullName ?? String.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var audiences = _configuration.GetSection("Jwt:Audiences").Get<string[]>() ?? new string[] { };
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "30"));
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            token.Payload["aud"] = audiences;

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                Expiration = expires,
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    Roles = roles
                }
            });
        }
    }
}
