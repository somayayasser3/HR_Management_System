using HrManagementSystem.Models;

namespace HrManagementSystem.Services
{
    public class SalaryReportServiceEF
    {
        private readonly HRContext _context;

        public SalaryReportServiceEF(HRContext context)
        {
            _context = context;
        }

        public async Task GenerateSalaryReportsAsync()
        {
            await _context.GenerateMonthlySalaryReportsAsync();
        }
        public async Task GenerateMonthlySalaryReportForEmployee(int month, int year, int employeeId)
        {
            await _context.GenerateMonthlySalaryReportForEmployee(month,year,employeeId);
        }
        public async Task GenerateMonthlySalaryReportForAllEmployeesInSpecificDate22(int month, int year)
        {
            await _context.GenerateMonthlySalaryReportForAllEmployeesInSpecificDate2(month,year);
        }
    }
}
