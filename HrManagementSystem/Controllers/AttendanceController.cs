using AutoMapper;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.DTOs.AuthDTOs;
using HrManagementSystem.Helper;
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
        public AttendanceController(UnitOfWork u, IMapper map)
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
                return NotFound(new { message = "No Attendance Founded."});
            }
            return Ok(AllEmpsAttendance);
        }

        [HttpGet("employee/{empid}")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult GetAttendanceForEmployee(int empid)
        {
            List<GetAttendaceDTO> EmpAttendance = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetAttendanceForEmployee(empid));
            if (unit.EmployeeRepo.getByID(empid) == null) return NotFound(new { message = "No such employee" });
            if (EmpAttendance.Count == 0)
            {
                return NotFound(new { message = "Employee doesn't have any attendance" });
            }
            return Ok(EmpAttendance);

        }


        [HttpGet("my-attendance")]
        [Authorize(Roles = "Employee,HR,Admin")]
        [EndpointSummary("Get current employee's attendance")]
        public IActionResult GetMyAttendance()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var employee = unit.EmployeeRepo.getAll().FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            List<GetAttendaceDTO> EmpAttendance = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetAttendanceForEmployee(employee.EmployeeId));
            return Ok(EmpAttendance);
        }


        [HttpPost("new")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public IActionResult AddAttendanceForEmployee(AddEmpAttendance dto)
        {
            double companyLat = 30.55816066274798;     
            double companyLong = 31.018756819985306;   
            double allowedRadius = 100;     

            if (dto == null /*|| dto.CheckInTime > dto.CheckOutTime*/)
                return BadRequest(new { message = "Invalid check-in or check-out time." });
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                            .Where(ms => ms.Value.Errors.Any())
                            .SelectMany(ms => ms.Value.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                return BadRequest(errors);
            }
            // get the location and compare it to the company Location 
            var distance = GeoHelper.CalculateDistanceInMeters(dto.Latitude, dto.Longitude,companyLat, companyLong);
            if (distance > allowedRadius)
            {
                return BadRequest(new { message = "You are outside the allowed location range." });
            }

            TimeSpan requiredCheckIn = unit.EmployeeRepo.GetStartTimeForEmployee(dto.EmployeeId); // 8:00 AM
            TimeSpan requiredCheckOut = unit.EmployeeRepo.GetEndTimeForEmployee(dto.EmployeeId); // 4:00 PM

            TimeSpan actualCheckIn = dto.CheckInTime;
            //TimeSpan? actualCheckOut = dto.CheckOutTime;
            decimal DelayHours = 0;
            decimal OvertimeHours = 0;
            if (dto.CheckInTime > requiredCheckIn)
                DelayHours = (decimal)(actualCheckIn - requiredCheckIn).TotalHours;

                //if (dto.CheckOutTime > requiredCheckOut)
                //    OvertimeHours = (decimal)(actualCheckOut - requiredCheckOut)?.TotalHours;


                Attendance newAttendance = mapper.Map<Attendance>(dto);
            try
            {
                newAttendance.OvertimeHours = OvertimeHours;
                newAttendance.DelayHours = DelayHours;
                newAttendance.CreatedAt = DateTime.Now;
                newAttendance.AttendanceDate = DateTime.Now;
                newAttendance.UpdatedAt= DateTime.Now;
                unit.AttendanceRepo.Add(newAttendance);
                unit.Save();
                return Ok(new { message = "Attendance added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Try again" });
            }

        }
        
        [HttpPut]
        public IActionResult UpdateAttendanceForEmployee(UpdateEmployeeAttendance dto)
        {


            double companyLat = 30.55816066274798;
            double companyLong = 31.018756819985306;

            double allowedRadius = 100;

            Attendance AttendanceToUpdate = unit.AttendanceRepo.GetSingleAttendanceForEmployeeByEmployeeIdandDate(dto.EmployeeId, DateTime.Now.Date);

            if (AttendanceToUpdate == null) return BadRequest(new { message = "No such attendance" });
            if (dto == null || AttendanceToUpdate.CheckInTime >= dto.CheckOutTime)
                return BadRequest(new { message = "Invalid check-in or check-out time." });

            TimeSpan requiredCheckIn = unit.EmployeeRepo.GetStartTimeForEmployee(dto.EmployeeId); // 8:00 AM
            TimeSpan requiredCheckOut = unit.EmployeeRepo.GetEndTimeForEmployee(dto.EmployeeId); // 4:00 PM

            TimeSpan actualCheckIn = AttendanceToUpdate.CheckInTime;
            TimeSpan? actualCheckOut = dto.CheckOutTime;

            // get the location and compare it to the company Location 
            var distance = GeoHelper.CalculateDistanceInMeters(dto.Latitude, dto.Longitude, companyLat, companyLong);
            if (distance > allowedRadius)
            {
                return BadRequest(new { message = "You are outside the allowed location range." });
            }

            decimal DelayHours = 0;
            decimal OvertimeHours = 0;
            if (AttendanceToUpdate.CheckInTime > requiredCheckIn)
                DelayHours = (decimal)(actualCheckIn - requiredCheckIn).TotalHours;

            if (dto.CheckOutTime > requiredCheckOut)
                OvertimeHours = (decimal)(actualCheckOut - requiredCheckOut)?.TotalHours;

            mapper.Map(dto, AttendanceToUpdate);
            try
            {
                AttendanceToUpdate.OvertimeHours = OvertimeHours;
                AttendanceToUpdate.DelayHours = DelayHours;
                AttendanceToUpdate.UpdatedAt = DateTime.Now;
                unit.AttendanceRepo.Update(AttendanceToUpdate);
                GetAttendaceDTO Updated = mapper.Map<GetAttendaceDTO>(unit.AttendanceRepo.GetSingleAttendanceForEmployee(AttendanceToUpdate.AttendanceId));
                unit.Save();

                return Ok(Updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Try again" });
            }
        }

        [HttpPut("Updateatt")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult AdminUpdatesAttendance(AdminUpdatesAttendanceDTO att)
        {
            Attendance oldAttendance  = unit.AttendanceRepo.getByID(att.AttendanceId);
            if (att == null || att.CheckInTime > att.CheckOutTime)
                return BadRequest(new { message = "Invalid check-in or check-out time." });
            if(att.AttendanceDate > DateTime.Now.Date)
                return BadRequest(new { message = "Invalid check-in or check-out time." });

            TimeSpan requiredCheckIn = unit.EmployeeRepo.GetStartTimeForEmployee(att.EmployeeId); // 8:00 AM
            TimeSpan requiredCheckOut = unit.EmployeeRepo.GetEndTimeForEmployee(att.EmployeeId); // 4:00 PM


            TimeSpan actualCheckIn = att.CheckInTime;
            TimeSpan? actualCheckOut = att.CheckOutTime;
            decimal DelayHours = 0;
            decimal OvertimeHours = 0;
            if (att.CheckInTime >= requiredCheckIn)
                att.DelayHours = (decimal)(actualCheckIn - requiredCheckIn).TotalHours;
            else
                att.DelayHours = 0;

            if (att.CheckOutTime >= requiredCheckOut)
                att.OvertimeHours = (decimal)(actualCheckOut - requiredCheckOut)?.TotalHours;
            else
                att.OvertimeHours= 0;
            try
            {
            mapper.Map(att, oldAttendance);
            unit.AttendanceRepo.Update(oldAttendance);
            unit.Save();    
            return Ok(att);

            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Try again" });
            }
            
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult DeleteAttendanceRow(int id)
        {
            Attendance attendance = unit.AttendanceRepo.getByID(id);
            if (attendance == null)
            {
                return BadRequest(new { message = "Wrong ID" });
            }
            try
            {
            unit.AttendanceRepo.Delete(id);
            unit.Save();
            return Ok(new { message = "Deleted" });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Try again" });
            }
        }

        

    }
}
