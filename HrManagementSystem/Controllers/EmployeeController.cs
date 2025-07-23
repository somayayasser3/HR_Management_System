using AutoMapper;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.DTOs.EmployeeDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Ensure that only authenticated users can access this controller
    public class EmployeeController : ControllerBase
    {
        UnitOfWork unit;
        IMapper mapper;
        UserManager<User> _userManager;

        public EmployeeController(IMapper Map, UnitOfWork unitofWork, UserManager<User> userManager)
        {
            this.mapper = Map;
            this.unit = unitofWork;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        [EndpointSummary("Get ALl Employees")]
        public IActionResult GetAllEmployees()
        {
            var employees = unit.EmployeeRepo.GetEmployeesandDepartment();
            var mappedEmps = mapper.Map<List<DisplayEmployeeData>>(employees);
            return Ok(mappedEmps);
        }
        [HttpGet("{EmpID}")]
        [Authorize(Roles = "Admin,HR")]
        [EndpointSummary("Get Employee by ID")]
        public IActionResult GetEmployeeByID(int EmpID)
        {
            var employee = unit.EmployeeRepo.GetEmployeeWithDeptBYID(EmpID);
            var mappedEmployee = mapper.Map<DisplayEmployeeData>(employee);
            return Ok(mappedEmployee);
        }
        ////////////////////////////////// 
        [HttpGet("my-profile")]
        [Authorize(Roles = "Employee")]
        [EndpointSummary("Get current employee's profile")]
        public IActionResult GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var employee = unit.EmployeeRepo.getAll().FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
                return NotFound("Employee profile not found");

            var mappedEmployee = mapper.Map<DisplayEmployeeData>(employee);
            return Ok(mappedEmployee);
        }

        [HttpPut("update-profile")]
        [Authorize(Roles = "Employee")]
        [EndpointSummary("Update employee profile (Employee only - Address and Phone)")]
        public IActionResult UpdateMyProfile([FromBody] UpdateEmployeeProfileDTO updateDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var employee = unit.EmployeeRepo.getAll().FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
                return NotFound("Employee profile not found");

            employee.Address = updateDto.Address;
            employee.PhoneNumber = updateDto.PhoneNumber;
            employee.UpdatedAt = DateTime.UtcNow;

            unit.EmployeeRepo.Update(employee);
            unit.Save();

            return Ok(new { message = "Profile updated successfully" });
        }

        //////////////////////////////////////////////////////////////
        [HttpPost]
        //[Authorize(Roles = "HR")]
        [EndpointSummary("Add Employee/User ")]
        public async Task<IActionResult> AddEmployeeAsync(AddEmployee Emp)
        {
            var user = mapper.Map<User>(Emp);
            user.Role = UserRole.Employee; // Set the role to Employee

            var AddUser = await _userManager.CreateAsync(user, Emp.Password);

            if (!AddUser.Succeeded)
                return BadRequest(AddUser.Errors);

            await _userManager.AddToRoleAsync(user, (UserRole.Employee).ToString());

            var MappedEmployee = mapper.Map<Employee>(Emp);
            MappedEmployee.UserId = user.Id;
            MappedEmployee.CreatedAt = DateTime.UtcNow;
            MappedEmployee.UpdatedAt = DateTime.UtcNow;


            if (Emp.Image == null || Emp.Image.Length == 0)
                return BadRequest("No file uploaded.");

            // Generate a unique file name
            var fileName = Emp.NationalId;
            var extension = Path.GetExtension(Emp.Image.FileName);
            var uniqueFileName = $"{fileName}{extension}";

            // Path to wwwroot/uploads (make sure this folder exists)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            MappedEmployee.ImagePath = filePath;
            unit.EmployeeRepo.Add(MappedEmployee);
            unit.Save();

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Emp.Image.CopyToAsync(stream);
            }
            
           
            var leaveBalance = new EmployeeLeaveBalance
            {
                EmployeeId = MappedEmployee.EmployeeId,
                AnnualLeaveBalance = 21,
                SickLeaveBalance = 15,
                UnpaidLeaveBalance = 0
            };
            unit.EmployeeLeaveBalanceRepo.Add(leaveBalance);
            unit.Save();
            return Ok("Employee Added Successfully");
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "HR")]
        [EndpointSummary("Edit Employee/User by ID")]
        public async Task<IActionResult> EditEmployeeAsync(int id, AddEmployee Emp)
        {
            var existingEmployee = unit.EmployeeRepo.GetEmployeeWithDeptBYID(id);
            if (existingEmployee == null)
                return NotFound();


            if (Emp.Image == null || Emp.Image.Length == 0)
                return BadRequest("No file uploaded.");

            // Generate a unique file name
            var fileName = Path.GetFileNameWithoutExtension(Emp.Image.FileName);
            var extension = Path.GetExtension(Emp.Image.FileName);
            var uniqueFileName = $"{fileName}{extension}";

            // Path to wwwroot/uploads (make sure this folder exists)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Emp.Image.CopyToAsync(stream);
            }


            var existingUser = await _userManager.FindByIdAsync(existingEmployee.UserId.ToString());
            if (existingUser == null)
                return NotFound("Linked User not found");

            mapper.Map<AddEmployee, User>(Emp, existingUser);

            var userUpdate = await _userManager.UpdateAsync(existingUser);
            if (!userUpdate.Succeeded)
                return BadRequest(userUpdate.Errors);

            mapper.Map<AddEmployee, Employee>(Emp, existingEmployee);
            existingEmployee.UpdatedAt = DateTime.Now;
            existingEmployee.ImagePath = filePath;
            unit.EmployeeRepo.Update(existingEmployee);
            unit.Save();

            return Ok("Edited Successfully");

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HR")]
        [EndpointSummary("Delete Employee/user by ID")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = unit.EmployeeRepo.getByID(id);
            if (employee == null)
                return NotFound();

            unit.EmployeeRepo.Delete(id);
            unit.Save();

            var user = _userManager.FindByIdAsync(employee.UserId.ToString()).Result;
            if (user != null)
            {
                _userManager.DeleteAsync(user).Wait();
            }

            // soft delete the user should have the its data still in system 

            return Ok(employee);
        }
    }
}
