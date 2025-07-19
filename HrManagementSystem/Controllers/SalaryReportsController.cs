using AutoMapper;
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
    [Authorize]
    public class SalaryReportsController : ControllerBase
    {
        UnitOfWork unit;
        IMapper mapper;
        public SalaryReportsController(UnitOfWork u,IMapper map)
        {
            unit = u;
            mapper = map;
        }

        [HttpGet("all")]
        [EndpointSummary("Get All salary reports")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult AllReports() 
        {
            List<GetSalaryReportDTO> AllSalaryReports = mapper.Map<List<GetSalaryReportDTO>>(unit.SalaryReportRepo.GetAllReportsWithEmps()); 
            return Ok(AllSalaryReports);
        }

        [HttpGet("employee/{empid}/month/{month}")]
        [EndpointSummary("Get salary report for specific employee in a specific month")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult GetEmployeeSalarReportInMonth(int empid , int month) 
        {
            GetSalaryReportDTO empSalaryReport =  mapper.Map<GetSalaryReportDTO>(unit.SalaryReportRepo.GetSalaryMonthReportWithEmployee(month,empid));
            if (empSalaryReport == null)
            {
                return BadRequest("Something went wrong");
            }
           
            return Ok(empSalaryReport);
        }
        [HttpGet("employee/{empid}")]
        [EndpointSummary("Get all salary report for specific employee")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult GetAllReportsForEmployee(int empid ) 
        {
            List<GetSalaryReportDTO> empSalaryReport =  mapper.Map<List<GetSalaryReportDTO>>(unit.SalaryReportRepo.GetAllReportsForEmp(empid));
            if (empSalaryReport == null)
            {
                return BadRequest("Something went wrong");
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
            return NotFound("Report Not Found");
            }
            unit.SalaryReportRepo.Delete(id);
            unit.Save();
            return Ok();    
        }

        [HttpPost("generate")]
        [EndpointSummary("Manually generate salary reports for the previous month")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GenerateMonthlySalaryReports(
    [FromServices] SalaryReportServiceEF salaryReportService)
        {
            await salaryReportService.GenerateSalaryReportsAsync();
            return Ok("Monthly salary reports generated suc cessfully.");
        }
    }
}
