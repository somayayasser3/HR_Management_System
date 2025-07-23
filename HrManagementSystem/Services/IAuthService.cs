using HrManagementSystem.DTOs.AuthDTOs;
using HrManagementSystem.DTOs.Employee;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync([FromForm] LoginDTO loginDto);
        Task<AuthResponseDTO> RegisterHRAsync(AddEmployee registerHREmployee);
        string GenerateJwtToken(string email, string role, string fullName, int userId,int id);
    }
}
