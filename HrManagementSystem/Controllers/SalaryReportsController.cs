using AutoMapper;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.DTOs.SalaryReportsDTOs;
using HrManagementSystem.Services;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SalaryReportsController : ControllerBase
    {
        UnitOfWork unit;
        IMapper mapper;
        private readonly SalaryReportServiceEF _salaryReportService;
        public SalaryReportsController(UnitOfWork u,IMapper map, SalaryReportServiceEF Sr)
        {
            unit = u;
            mapper = map;
            _salaryReportService = Sr;

        }

        [HttpGet("all")]
        [EndpointSummary("Get All salary reports")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult AllReports() 
        {
            List<GetSalaryReportDTO> AllSalaryReports = mapper.Map<List<GetSalaryReportDTO>>(unit.SalaryReportRepo.GetAllReportsWithEmps()); 
            return Ok(AllSalaryReports);
        }

        [HttpPost("SpecificSalary")]
        [EndpointSummary("Get salary report for specific employee in a specific month")]
        //[Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetEmployeeSalarReportInMonth(GetSalaryReportForSpecificEmployeeDTO salaryReportInfo)
        {
            GetSalaryReportDTO empSalaryReport = mapper.Map<GetSalaryReportDTO>(await unit.SalaryReportRepo.GetSalaryMonthReportWithEmployee(salaryReportInfo.Month, salaryReportInfo.Year, salaryReportInfo.EmployeeId));
            if (empSalaryReport == null)
            {
                return BadRequest(new { message = "Something went wrong" });
            }

            return Ok(empSalaryReport);
        }

        [HttpPost("DetailedSalary")]
        [EndpointSummary("Get Detailed salary report for specific employee in a specific month")]
        //[Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetEmployeeDetailedSalarReportInMonth(GetSalaryReportForSpecificEmployeeDTO salaryReportInfo)
        {
           DetailedAttendanceDTO detailed = new DetailedAttendanceDTO();
            detailed.attendances = mapper.Map<List<GetAttendaceDTO>>(unit.AttendanceRepo.GetOvertimeAndDelayDaysInSpecificDate(salaryReportInfo.EmployeeId, salaryReportInfo.Month, salaryReportInfo.Year));
            detailed.DelaySummation = detailed.attendances.Sum(a => a.DelayHours); 
            detailed.OverTimeSummation = detailed.attendances.Sum(a => a.OvertimeHours);
            GetSalaryReportDTO sr = mapper.Map<GetSalaryReportDTO>(await unit.SalaryReportRepo.GetSalaryMonthReportWithEmployee(salaryReportInfo.Month, salaryReportInfo.Year, salaryReportInfo.EmployeeId));
            detailed.OvertimeAmount = sr.OvertimeAmount;
            detailed.DelayAmount = sr.DeductionAmount;
            return Ok(detailed);
        }






        [HttpGet("employee/{empid}")]
        [EndpointSummary("Get all salary report for specific employee")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult GetAllReportsForEmployee(int empid ) 
        {
            List<GetSalaryReportDTO> empSalaryReport =  mapper.Map<List<GetSalaryReportDTO>>(unit.SalaryReportRepo.GetAllReportsForEmp(empid));
            if (empSalaryReport == null)
            {
                return BadRequest(new { message = "Something went wrong" });
            }

            return Ok(empSalaryReport);
        }
        [HttpDelete("delete/{id}")]
        [EndpointSummary("Delete salary report by ID")]
        [Authorize(Roles = "HR")]
        public IActionResult DeleteSalaryReport(int id)
        {
            GetSalaryReportDTO report = mapper.Map<GetSalaryReportDTO>(unit.SalaryReportRepo.getByID(id));
            if (report == null) 
            { 
            return NotFound(new { message = "Report Not Found" });
            }
            try
            {

            unit.SalaryReportRepo.Delete(id);
            unit.Save();
            return Ok();    
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }

        [HttpPost("generate")]
        [EndpointSummary("Manually generate salary reports for the previous month")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GenerateMonthlySalaryReports()
        {
            try
            {

            await _salaryReportService.GenerateSalaryReportsAsync();
            return Ok(new { message = "Monthly salary reports generated suc cessfully." });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }

        [HttpPost("generateEMP")]
        [EndpointSummary("Manually generate salary reports for Employee in specific month and year")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GenerateMonthlySalaryReportForEmployee(int m,int y , int id)
        {
            try
            {

            await _salaryReportService.GenerateMonthlySalaryReportForEmployee(m,y,id);
            return Ok(new { message = "Monthly salary reports generated suc cessfully." });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }


       
    }
}
