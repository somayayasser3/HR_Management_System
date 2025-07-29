using AutoMapper;
using HrManagementSystem.Controllers;
using HrManagementSystem.DTOs.AuthDTOs;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        IFaceRecognitionService faceRecognitionService;

        public AuthService(IFaceRecognitionService f,IMapper m, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, IOptions<JwtSettings> jwtSettings, UnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            mapper = m;
            faceRecognitionService = f;
        }

        public async Task<AuthResponseDTO> LoginAsync([FromForm]LoginDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var DBImage = GetFormFileFromWwwRoot(_unitOfWork.EmployeeRepo.GetEmployeeByUserId(user.Id).ImagePath);
            CompareFacesDto c = new CompareFacesDto()
            {
                Image1 = DBImage,
                Image2 = loginDto.Image,
            };
            var faceRecognitionResult = await faceRecognitionService.CompareFacesAsync(c);
            if (!faceRecognitionResult.Success || !faceRecognitionResult.IsSamePerson)
            {
                throw new UnauthorizedAccessException("Face does not match");
            }
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Employee";
            
            var e = _unitOfWork.EmployeeRepo.GetEmployeeByUserId(user.Id);

            var token = GenerateJwtToken(user.Email, role, user.FullName, user.Id,e.EmployeeId);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                EmployeeId = e.EmployeeId.ToString()??"0"
            };
        }

        public async Task<AuthResponseDTO> RegisterHRAsync(AddEmployee registerHREmployee)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerHREmployee.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("DuplicateEmail");
            }

            var existingUserWithPhone = _unitOfWork.EmployeeRepo.GetExistingByPhoneNumber(registerHREmployee.PhoneNumber);
            if (existingUserWithPhone != null)
            {
                throw new InvalidOperationException("Duplicate phone number"); ;
            }
            var existingUserWithNatID = _unitOfWork.EmployeeRepo.GetExistingByNationalID(registerHREmployee.NationalId);
            if (existingUserWithNatID != null)
            {
                throw new InvalidOperationException("Duplicate National ID");
            }

            var user = mapper.Map<User>(registerHREmployee);
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.Role = UserRole.HR;
            var result = await _userManager.CreateAsync(user, registerHREmployee.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await _userManager.AddToRoleAsync(user, (UserRole.HR).ToString());


            var hrEmployee = mapper.Map<Employee>(registerHREmployee);
            hrEmployee.UserId = user.Id;
            hrEmployee.CreatedAt = DateTime.UtcNow;
            hrEmployee.UpdatedAt = DateTime.UtcNow;

            if (registerHREmployee.Image == null || registerHREmployee.Image.Length == 0)
                throw new InvalidOperationException("Image not sent");
            // Generate a unique file name
            var fileName = registerHREmployee.NationalId;
            var extension = Path.GetExtension(registerHREmployee.Image.FileName);
            var uniqueFileName = $"{fileName}{extension}";

            // Path to wwwroot/uploads (make sure this folder exists)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await registerHREmployee.Image.CopyToAsync(stream);
            }

            hrEmployee.ImagePath = filePath;
            _unitOfWork.EmployeeRepo.Add(hrEmployee);
            _unitOfWork.Save();

            var token = GenerateJwtToken(user.Email, "HR", user.FullName, user.Id,hrEmployee.EmployeeId);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                Role = "HR",
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                EmployeeId= hrEmployee.EmployeeId.ToString()??"0"
            };
        }

        public string GenerateJwtToken(string email, string role, string fullName, int userId,int id)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, fullName),
                new Claim("EmployeeId",id.ToString()),
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
        public IFormFile GetFormFileFromWwwRoot(string relativePath)
        {
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","uploads", relativePath);
            var stream = new FileStream(relativePath, FileMode.Open, FileAccess.Read);
            return new FormFile(stream, 0, stream.Length, Path.GetFileName(relativePath), Path.GetFileName(relativePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }
    }
}