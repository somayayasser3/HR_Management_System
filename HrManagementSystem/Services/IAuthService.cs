using HrManagementSystem.DTOs.AuthDTOs;

namespace HrManagementSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RegisterHRAsync(RegisterHRDTO registerDto);
        string GenerateJwtToken(string email, string role, string fullName, int userId);
    }
}
