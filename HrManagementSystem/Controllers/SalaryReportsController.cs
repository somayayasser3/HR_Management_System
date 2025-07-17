using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HrManagementSystem.DTOs.SalaryReportsDTOs;
using AutoMapper;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult AllReports() 
        {
            List<GetSalaryReportDTO> AllSalaryReports = mapper.Map<List<GetSalaryReportDTO>>(unit.SalaryReportRepo.GetAllReportsWithEmps()); 
            return Ok(AllSalaryReports);
        }

        [HttpGet("employee/{empid}/month/{month}")]
        [EndpointSummary("Get salary report for specific employee in a specific month")]
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
    }
}
