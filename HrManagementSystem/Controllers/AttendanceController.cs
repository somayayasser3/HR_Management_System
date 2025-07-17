using AutoMapper;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult GetAttendanceForEmployee(int empid)
        {
            List<GetAttendaceDTO> EmpAttendance = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetAttendanceForEmployee(empid));
            if (EmpAttendance.Count == 0)
            {
                return NotFound();
            }
            return Ok(EmpAttendance);

        }
        [HttpPost("new")]
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
