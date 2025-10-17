using ITI_Project.BLL.Common;
using ITI_Project.BLL.DTOs;
using ITI_Project.BLL.Interfaces;
using ITI_Project.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ITI_Project.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<Result<LoginResultDto>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || string.IsNullOrEmpty(user.UserName))
            {
                return Result<LoginResultDto>.Failure("Invalid email or password.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

            if (!isPasswordValid)
            {
                return Result<LoginResultDto>.Failure("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // FIX: Read audiences using GetChildren() instead of Get<string[]>()
            var audiences = _configuration.GetSection("Jwt:Audiences")
                .GetChildren()
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
            
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "30"));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            token.Payload["aud"] = audiences;

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var result = new LoginResultDto
            {
                Token = tokenString,
                Expiration = expires,
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Roles = roles.ToList()
                }
            };

            return Result<LoginResultDto>.Success(result);
        }

        public async Task<Result> RegisterAsync(RegisterUserDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return Result.Failure("Password and Confirm Password do not match.");
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUserByEmail is not null)
            {
                return Result.Failure("Email already exists.");
            }

            var existingUserByUserName = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUserByUserName is not null)
            {
                return Result.Failure("UserName already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FullName = registerDto.Name
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return Result.Success();
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result.Failure(errors);
        }
    }
}