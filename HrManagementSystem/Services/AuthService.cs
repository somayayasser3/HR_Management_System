using AutoMapper;
using HrManagementSystem.DTOs.AuthDTOs;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HrManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly JwtSettings _jwtSettings;
        private readonly UnitOfWork _unitOfWork;
        IMapper mapper;

        public AuthService(IMapper m ,UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, IOptions<JwtSettings> jwtSettings, UnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            mapper = m;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Employee";

            var token = GenerateJwtToken(user.Email, role, user.FullName, user.Id);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }

        public async Task<AuthResponseDTO> RegisterHRAsync(AddEmployee registerHREmployee)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerHREmployee.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            var user = mapper.Map<User>(registerHREmployee);

            //{
            //    UserName = registerDto.Email,
            //    Email = registerDto.Email,
            //    FullName = registerDto.FullName,
            //    PhoneNumber = registerDto.PhoneNumber,
            //    Address = "HR Office", // Default address
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.Role = UserRole.HR;
            //    Role = UserRole.HR // Set role to HR
            //};

            // Assign HR role
            var result = await _userManager.CreateAsync(user, registerHREmployee.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await _userManager.AddToRoleAsync(user, "HR");


            var hrEmployee = new Employee
                        {
                            FullName = registerHREmployee.FullName,
                            Address = registerHREmployee.Address,
                            PhoneNumber = registerHREmployee.PhoneNumber,
                            NationalId = registerHREmployee.NationalId, // Default value
                            Gender = registerHREmployee.Gender,
                            HireDate = DateTime.Now,
                            Salary = registerHREmployee.Salary, // Default salary
                            WorkStartTime = DateTime.Today.AddHours(8), // 8 AM
                            WorkEndTime = DateTime.Today.AddHours(16), // 4 PM
                            DepartmentId = 2, // Default department - make sure this exists
                            UserId = user.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,                            

                        };                       

            _unitOfWork.EmployeeRepo.Add(hrEmployee);
            _unitOfWork.Save();

            var token = GenerateJwtToken(user.Email, "HR", user.FullName, user.Id);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                Role = "HR",
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }

        public string GenerateJwtToken(string email, string role, string fullName, int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}