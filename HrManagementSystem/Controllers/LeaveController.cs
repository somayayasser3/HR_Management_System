using AutoMapper;
using HrManagementSystem.DTOs.LeaveDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : Controller
    {
        UnitOfWork unit;
        IMapper mapper;
        public LeaveController(UnitOfWork unitOfWork, IMapper mapper)
        {
            this.unit = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet("types")]
        public IActionResult GetAllLeaveTypes()
        {
            var types = unit.LeaveRepo.GetAllTypes();
            return Ok(types);
        }

        // for HR view all Req (pending - approved - rejected)
        [HttpGet("requests")]
        public IActionResult GetAllRequests()
        {
            var Requests = unit.LeaveRepo.GetAllRequests();
            var mappedRequests = mapper.Map<List<DisplayResultforLeaveRequest>>(Requests);
            return Ok(mappedRequests);
        }

        // employee adding a request 
        [HttpPost]
        public IActionResult AddLeaveRequest(AddLeaveRequest Req)
        {
            var leaveType = unit.LeaveRepo.GetLeaveTypeofReq(Req.LeaveTypeId);
            var emp = unit.EmployeeRepo.GetEmployeeWithDeptBYID(Req.EmployeeId);
            LeaveRequest request = mapper.Map<LeaveRequest>(Req);

            DateTime start = request.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime end = request.EndDate.ToDateTime(TimeOnly.MinValue);
            DateTime today = DateTime.Today;

            if (start < today || end < today || request.EndDate < request.StartDate)
            {
                return BadRequest(new { message = "You cannot request leave for a past date." });
            }
            
            if (unit.LeaveRepo.FoundLeaveRequest(Req.EmployeeId))
            {
                return BadRequest(new { message = "You have pending leave request" });
            }
            var days = (request.EndDate.ToDateTime(TimeOnly.MinValue) - request.StartDate.ToDateTime(TimeOnly.MinValue)).Days +1;
            int daysForThatTypeBefore = unit.LeaveRepo.LeaveRequestTypeApprovedCountForEmployee(Req.EmployeeId,Req.LeaveTypeId);
            
            if(daysForThatTypeBefore>=leaveType.MaxDaysPerYear)
                return BadRequest(new { message = "You have took all allowed days of this leave this year" });
            
            
            if (daysForThatTypeBefore + days > leaveType.MaxDaysPerYear)
                return BadRequest(new { message = $"You can only take {leaveType.MaxDaysPerYear - daysForThatTypeBefore } day(s) leave" });
            request.RequestDaysCount = days;            
            try
            {
            unit.LeaveRepo.Add(request);
            unit.Save();
            return Ok(new { message = " Request added successfully" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }

        }
           
        [HttpPut("{ReqID}")]
        public IActionResult HandleRequest(int ReqID, string Status)
        {
            var request = unit.LeaveRepo.getByID(ReqID);
            if (request == null)
            {
                return NotFound(new { message = "Request not found" });
            }
            if (request.Status == "Pending")
            {
                request.Status = Status;
                try
                {

                unit.LeaveRepo.Update(request);
                unit.Save();
                return Ok(new { message = "Request handled successfully", status = request.Status });
                }
                catch (Exception)
                {
                    return BadRequest(new { message = "Try again" });
                }
            }
            else
            {
                return BadRequest(new { message = "Request has already been handled" });
            }
        }
    }
}


