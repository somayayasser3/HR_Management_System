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
            //  if Entered Past Date
            if (start < today || end < today)
            {
                return BadRequest(new { message = "You cannot request leave for a past date." });
            }
            var days = (request.EndDate.ToDateTime(TimeOnly.MinValue) - request.StartDate.ToDateTime(TimeOnly.MinValue)).Days +1;
            // if Enter days more than leave balance
            switch (leaveType.Name.ToLower())
            {
                case "annual":
                     if ( emp.LeaveBalance.AnnualLeaveBalance < days)
                    {
                        request.Status = "Rejected";
                        return BadRequest(new { message = "Insufficient annual leave balance", Status = request.Status });
                    }
                    break;
                case "sick":
                    if (emp.LeaveBalance.SickLeaveBalance < days)
                    {
                        request.Status = "Rejected";
                        return BadRequest(new { message = "Insufficient sick leave balance", Status = request.Status });
                    }
                    break;
                case "unpaid":
                    break;
            }

            unit.LeaveRepo.Add(request);
            unit.Save();
            return Ok(new { message = " Request added successfully" });

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

                if (request.Status.ToLower() == "approved")
                {
                    var days = (request.EndDate.ToDateTime(TimeOnly.MinValue) - request.StartDate.ToDateTime(TimeOnly.MinValue)).Days +1 ;
                    var employeeLeaveBalance = unit.EmployeeLeaveBalanceRepo.EmpBalanceByID(request.EmployeeId);
                    var leaveReq = unit.LeaveRepo.GetRequestByID(request.Id);

                    switch (leaveReq.LeaveType.Name.ToLower())
                    {
                        case "annual":
                            employeeLeaveBalance.AnnualLeaveBalance -= days;
                            break;
                        case "sick":
                            employeeLeaveBalance.SickLeaveBalance -= days;
                            break;
                        case "unpaid":
                            employeeLeaveBalance.UnpaidLeaveBalance += days;
                            break;
                    }


                    if (request.Status.ToLower() != "rejected")
                    {
                        for (int i = 0; i < days; i++)
                        {
                            var leaveDate = request.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(i);
                            var attendance = new Attendance
                            {
                                EmployeeId = request.EmployeeId,
                                AttendanceDate = leaveDate,
                                CheckInTime = employeeLeaveBalance.Employee.WorkStartTime.TimeOfDay,
                                CheckOutTime = employeeLeaveBalance.Employee.WorkEndTime.TimeOfDay,
                                OvertimeHours = 0,
                                DelayHours = 0,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            unit.AttendanceRepo.Add(attendance);
                        }

                    }
                    unit.EmployeeLeaveBalanceRepo.Update(employeeLeaveBalance);

                }



                unit.LeaveRepo.Update(request);
                unit.Save();
                return Ok(new { message = "Request handled successfully", status = request.Status });
            }
            else
            {
                return BadRequest(new { message = "Request has already been handled" });
            }
        }
    }
}


