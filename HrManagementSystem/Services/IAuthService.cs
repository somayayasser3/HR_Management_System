using HrManagementSystem.DTOs.AuthDTOs;
using HrManagementSystem.DTOs.Employee;

namespace HrManagementSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RegisterHRAsync(AddEmployee registerHREmployee);
        string GenerateJwtToken(string email, string role, string fullName, int userId,int id);
    }
}
