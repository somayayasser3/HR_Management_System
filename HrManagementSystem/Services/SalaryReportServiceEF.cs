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
    }
}
