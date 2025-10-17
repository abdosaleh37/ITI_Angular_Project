using ITI_Project.BLL.Common;
using ITI_Project.BLL.DTOs;

namespace ITI_Project.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResultDto>> LoginAsync(string email, string password);
        Task<Result> RegisterAsync(RegisterUserDto registerDto);
    }
}