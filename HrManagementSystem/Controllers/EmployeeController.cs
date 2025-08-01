using AutoMapper;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.DTOs.EmployeeDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure that only authenticated users can access this controller
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
          
            for(int i = 0; i < employees.Count; i++)
                if (employees[i].EmployeeId == 10)
                    { employees.RemoveAt(i); break; }
            
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
        [HttpGet("my-profile")]
        [Authorize(Roles = "Employee")]
        [EndpointSummary("Get current employee's profile")]
        public IActionResult GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var employee = unit.EmployeeRepo.GetEmployeeByUserId(userId);

            //var employee = unit.EmployeeRepo.getAll().FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
                return NotFound(new { message = "Employee profile not found" });

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
                return NotFound(new { message = "Employee profile not found" });

            var existingUserWithPhone = unit.EmployeeRepo.GetAnotherExistingByPhoneNumber(employee.EmployeeId, updateDto.PhoneNumber);
            if (existingUserWithPhone != null)
            {
                return BadRequest(new { message = "Duplicate phone number" });
            }
            try
            {

                employee.Address = updateDto.Address;
                employee.PhoneNumber = updateDto.PhoneNumber;
                employee.UpdatedAt = DateTime.UtcNow;
                unit.EmployeeRepo.Update(employee);
                unit.Save();
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }
        [HttpPost]
        [Authorize(Roles = "HR,Admin")]
        [EndpointSummary("Add Employee/User ")]
        public async Task<IActionResult> AddEmployeeAsync(AddEmployee Emp)
        {
            var user = mapper.Map<User>(Emp);
            user.Role = UserRole.Employee; // Set the role to Employee

            var existingUserWithPhone = unit.EmployeeRepo.GetExistingByPhoneNumber(Emp.PhoneNumber);
            if (existingUserWithPhone != null)
            {
                return BadRequest(new { message = "Duplicate phone number" });
            }
            var existingUserWithNatID = unit.EmployeeRepo.GetExistingByNationalID(Emp.NationalId);
            if (existingUserWithNatID != null)
            {
                return BadRequest(new { message = "Duplicate National ID" });
            }

            try
            {
                var AddUser = await _userManager.CreateAsync(user, Emp.Password);
                if (!AddUser.Succeeded)
                    return BadRequest(AddUser.Errors);
                await _userManager.AddToRoleAsync(user, (UserRole.Employee).ToString());

            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }


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
            try
            {
                unit.EmployeeRepo.Add(MappedEmployee);
                unit.Save();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Emp.Image.CopyToAsync(stream);
            }


            //var leaveBalance = new EmployeeLeaveBalance
            //{
            //    EmployeeId = MappedEmployee.EmployeeId,
            //    AnnualLeaveBalance = 21,
            //    SickLeaveBalance = 15,
            //    UnpaidLeaveBalance = 0
            //};
            try
            {
                //unit.EmployeeLeaveBalanceRepo.Add(leaveBalance);
                unit.Save();
                return Ok(new { message = "Employee Added Successfully" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "HR,Admin")]
        [EndpointSummary("Edit Employee/User by ID")]
        public async Task<IActionResult> EditEmployeeAsync(UpdateEmployeeDTO Emp)
        {
            var existingEmployee = unit.EmployeeRepo.GetEmployeeWithDeptBYID(Emp.EmployeeId);
            if (existingEmployee == null)
                return NotFound();

            var existingUserWithPhone = unit.EmployeeRepo.GetAnotherExistingByPhoneNumber(Emp.EmployeeId, Emp.PhoneNumber);
            if (existingUserWithPhone != null)
            {
                return BadRequest(new { message = "Duplicate phone number" });
            }
            var existingUserWithNatID = unit.EmployeeRepo.GetAnotherExistingByNationalID(Emp.EmployeeId, Emp.NationalId);
            if (existingUserWithNatID != null)
            {
                return BadRequest(new { message = "Duplicate National ID" });
            }


            string path = existingEmployee.ImagePath;
            try
            {

                var existingUser = await _userManager.FindByIdAsync(existingEmployee.UserId.ToString());
                if (existingUser == null)
                    return NotFound(new { message = "Linked User not found" });

                mapper.Map<UpdateEmployeeDTO, User>(Emp, existingUser);
                var userUpdate = await _userManager.UpdateAsync(existingUser);
                if (!userUpdate.Succeeded)
                    return BadRequest(userUpdate.Errors);

                mapper.Map<UpdateEmployeeDTO, Employee>(Emp, existingEmployee);
                existingEmployee.UpdatedAt = DateTime.Now;
                existingEmployee.ImagePath = path;
                unit.EmployeeRepo.Update(existingEmployee);
                unit.Save();
                return Ok(new { message = "Updated Successfully" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }

        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "HR,Admin")]
        [EndpointSummary("Delete Employee/user by ID")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = unit.EmployeeRepo.getByID(id);
            if (employee == null)
                return NotFound();
            try
            {
                unit.EmployeeRepo.Delete(id);
                unit.Save();
                var user = _userManager.FindByIdAsync(employee.UserId.ToString()).Result;
                if (user != null)
                {
                    _userManager.DeleteAsync(user).Wait();
                }
                return Ok(employee);
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }

        }
    }
}
