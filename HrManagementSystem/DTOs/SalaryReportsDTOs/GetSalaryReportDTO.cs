using HrManagementSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.DTOs.SalaryReportsDTOs
{
    public class GetSalaryReportDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal NetSalary { get; set; }
    }
}
