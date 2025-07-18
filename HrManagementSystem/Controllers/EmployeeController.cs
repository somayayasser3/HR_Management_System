using AutoMapper;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [EndpointSummary("Get ALl Employees")]
        public IActionResult GetAllEmployees()
        {
            var employees = unit.EmployeeRepo.getAll();
            var mappedEmps = mapper.Map<List<DisplayEmployeeData>>(employees);
            return Ok(mappedEmps);
        }
        [HttpGet("{EmpID}")]
        [EndpointSummary("Get Employee by ID")]
        public IActionResult GetEmployeeByID( int EmpID)
        {
            var employee = unit.EmployeeRepo.getByID(EmpID);
            var mappedEmployee = mapper.Map<DisplayEmployeeData>(employee);
            return Ok(mappedEmployee);
        }
        [HttpPost]
        [EndpointSummary("Add Employee/User ")]
        public async Task<IActionResult> AddEmployeeAsync(AddEmployee Emp)
        {
            var user = mapper.Map<User>(Emp);
            
            var AddUser = await _userManager.CreateAsync(user, Emp.Password);
            
            if (!AddUser.Succeeded)
                return BadRequest(AddUser.Errors);

            var MappedEmployee = mapper.Map<Employee>(Emp);
            MappedEmployee.UserId = user.Id;
            MappedEmployee.CreatedAt = DateTime.UtcNow;
            MappedEmployee.UpdatedAt = DateTime.UtcNow;

            unit.EmployeeRepo.Add(MappedEmployee);
            unit.Save();
            return Ok("Employee Added Successfully");
        }

        [HttpPut("{id}")]
        [EndpointSummary("Edit Employee/User by ID")]

        public async Task<IActionResult> EditEmployeeAsync( int id , AddEmployee Emp)
        {
            var existingEmployee = unit.EmployeeRepo.getByID(id);
            if (existingEmployee == null)
                return NotFound();

            var existingUser = await _userManager.FindByIdAsync(existingEmployee.UserId.ToString());
            if (existingUser == null)
                return NotFound("Linked User not found");

            mapper.Map<AddEmployee, User>(Emp, existingUser);
          
            var userUpdate = await _userManager.UpdateAsync(existingUser);
            if (!userUpdate.Succeeded)
                return BadRequest(userUpdate.Errors);

            mapper.Map<AddEmployee, Employee>(Emp, existingEmployee);
            existingEmployee.UpdatedAt = DateTime.Now;
            unit.EmployeeRepo.Update(existingEmployee);
            unit.Save();

            return Ok("Edited Successfully");

        }

        [HttpDelete("{id}")]
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
        } }
}
