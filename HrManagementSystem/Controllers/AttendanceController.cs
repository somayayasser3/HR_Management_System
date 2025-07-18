using AutoMapper;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        UnitOfWork unit;
        IMapper mapper;
        public AttendanceController(UnitOfWork u,IMapper map)
        {
            unit = u;
            mapper = map;
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult GetAllAttendance()
        {
            List<GetAttendaceDTO> AllEmpsAttendance = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetAttendanceWithEmployees());
            if (AllEmpsAttendance.Count == 0)
            {
                return NotFound();
            }
            return Ok(AllEmpsAttendance);
        }

        [HttpGet("employee/{empid}")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult GetAttendanceForEmployee(int empid)
        {
            List<GetAttendaceDTO> EmpAttendance = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetAttendanceForEmployee(empid));
            if (EmpAttendance.Count == 0)
            {
                return NotFound();
            }
            return Ok(EmpAttendance);

        }

        /// ////////////////////////////////////////

        [HttpGet("my-attendance")]
        [Authorize(Roles = "Employee")]
        [EndpointSummary("Get current employee's attendance")]
        public IActionResult GetMyAttendance()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var employee = unit.EmployeeRepo.getAll().FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
                return NotFound("Employee not found");

            List<GetAttendaceDTO> EmpAttendance = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetAttendanceForEmployee(employee.EmployeeId));
            return Ok(EmpAttendance);
        }
        /// /////////////////////////////////////////
        

        [HttpPost("new")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult AddAttendanceForEmployee(AddEmpAttendance dto)
        {

            if (dto == null || dto.CheckInTime >= dto.CheckOutTime)
                return BadRequest("Invalid check-in or check-out time.");

            TimeSpan requiredCheckIn = new TimeSpan(8, 0, 0); // 8:00 AM
            TimeSpan requiredCheckOut = new TimeSpan(16, 0, 0); // 4:00 PM

            TimeSpan actualCheckIn = dto.CheckInTime.TimeOfDay;
            TimeSpan actualCheckOut = dto.CheckOutTime.TimeOfDay;

            // Calculate delay & overtime
            decimal delayHours = 0;
            decimal overtimeHours = 0;

            if (actualCheckIn > requiredCheckIn)
                dto.DelayHours= (decimal)(actualCheckIn - requiredCheckIn).TotalHours;

            if (actualCheckOut > requiredCheckOut)
                dto.OvertimeHours = (decimal)(actualCheckOut - requiredCheckOut).TotalHours;

            dto.CreatedAt = DateTime.Now;
            dto.UpdatedAt= DateTime.Now;

            Attendance newAttendance = mapper.Map<Attendance>(dto);
            
            unit.AttendanceRepo.Add(newAttendance);
            unit.Save();
            return Ok("Attendance added successfully.");

        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult DeleteAttendanceRow(int id)
        {
            Attendance attendance = unit.AttendanceRepo.getByID(id);
            if(attendance == null)
            {
                return BadRequest();    
            }
            unit.AttendanceRepo.Delete(id);
            unit.Save();
            return Ok("Deleted");
        }



    }
}
