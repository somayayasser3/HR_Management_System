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
            if(unit.EmployeeRepo.getByID(empid)==null) return NotFound(new { message = "No such employee" });
            if (EmpAttendance.Count == 0)
            {
                return NotFound(new {message = "Employee doesn't have any attendance" } );
            }
            return Ok(EmpAttendance);

        }

        /// ////////////////////////////////////////

        [HttpGet("my-attendance")]
        [Authorize(Roles = "Employee,HR,Admin")]
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
                return BadRequest(new{ message = "Invalid check-in or check-out time." });
            if (!ModelState.IsValid)
            {
                    var errors = ModelState
                                .Where(ms => ms.Value.Errors.Any())
                                .SelectMany(ms => ms.Value.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
                return BadRequest(errors);
            }
            TimeSpan requiredCheckIn = new TimeSpan(8, 0, 0); // 8:00 AM
            TimeSpan requiredCheckOut = new TimeSpan(16, 0, 0); // 4:00 PM

            TimeSpan actualCheckIn = dto.CheckInTime;
            TimeSpan? actualCheckOut = dto.CheckOutTime;

            if (dto.CheckInTime > requiredCheckIn)
                dto.DelayHours= (decimal)(actualCheckIn - requiredCheckIn).TotalHours;

            if (dto.CheckOutTime > requiredCheckOut)
                dto.OvertimeHours = (decimal)(actualCheckOut - requiredCheckOut)?.TotalHours;
            dto.UpdatedAt= DateTime.Now;

            Attendance newAttendance = mapper.Map<Attendance>(dto);
            try
            {
            unit.AttendanceRepo.Add(newAttendance);
            unit.Save();
            return Ok(new { message = "Attendance added successfully." });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Try again" });
            }

        }
        [HttpPut]
        public IActionResult UpdateAttendanceForEmployee(UpdateEmployeeAttendance dto)
        {
            Attendance AttendanceToUpdate = unit.AttendanceRepo.getByID(dto.AttendanceId);

            if(AttendanceToUpdate==null) return BadRequest(new { message = "No such attendance" });
            if (dto == null || dto.CheckInTime >= dto.CheckOutTime)
                return BadRequest(new{ message = "Invalid check-in or check-out time." });
            if (!ModelState.IsValid)
            {
                    var errors = ModelState
                                .Where(ms => ms.Value.Errors.Any())
                                .SelectMany(ms => ms.Value.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
                return BadRequest(errors);
            }
            TimeSpan requiredCheckIn = new TimeSpan(8, 0, 0); // 8:00 AM
            TimeSpan requiredCheckOut = new TimeSpan(16, 0, 0); // 4:00 PM

            TimeSpan actualCheckIn = dto.CheckInTime;
            TimeSpan? actualCheckOut = dto.CheckOutTime;

            if (dto.CheckInTime > requiredCheckIn)
                dto.DelayHours= (decimal)(actualCheckIn - requiredCheckIn).TotalHours;

            if (dto.CheckOutTime > requiredCheckOut)
                dto.OvertimeHours = (decimal)(actualCheckOut - requiredCheckOut)?.TotalHours;

            //dto.CreatedAt = DateTime.Now;
            dto.UpdatedAt= DateTime.Now;

             mapper.Map(dto,AttendanceToUpdate);
            try
            {
            unit.AttendanceRepo.Update(AttendanceToUpdate);
            Attendance newAttendance = mapper.Map<Attendance>(dto);
            return Ok(new { message = "Attendance added successfully." });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Try again" });
            }
            unit.AttendanceRepo.Add(newAttendance);
            Attendance newAttendance = mapper.Map<Attendance>(dto);
            return Ok("Attendance added successfully.");
        [Authorize(Roles = "Admin,HR")]
            
            unit.AttendanceRepo.Add(newAttendance);
            
            Attendance newAttendance = mapper.Map<Attendance>(dto);
            unit.AttendanceRepo.Add(newAttendance);
            unit.Save();
                return BadRequest(new { message = "Wrong ID" });    
            catch(Exception ex)
            {
                return BadRequest();    
            return Ok(new {message = "Deleted" });
        [Authorize(Roles = "Admin,HR")]
        }
            return Ok("Deleted");
        [Authorize(Roles = "Admin,HR")]
        public IActionResult DeleteAttendanceRow(int id)

            
            Attendance attendance = unit.AttendanceRepo.getByID(id);
            if(attendance == null)
            {
                return BadRequest(new { message = "Wrong ID" });    
            }
            unit.AttendanceRepo.Delete(id);
            unit.Save();
            return Ok(new {message = "Deleted" });
        }

        

    }
}
