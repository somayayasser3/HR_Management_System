using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class SalaryReportRepositroy : GenericRepo<SalaryReport>
    {
        public SalaryReportRepositroy(HRContext context) : base(context){}
        public async Task<SalaryReport> GetSalaryMonthReportWithEmployee (int month , int year , int empId)
        {
            return await con.SalaryReports
                .AsNoTracking()
                .Include(x => x.Employee)
                .ThenInclude(e=>e.Department)
                .FirstOrDefaultAsync(x => x.Month == month && x.EmployeeId == empId && x.Year == year );
        }




        public List<SalaryReport> GetAllReportsWithEmps ()
        {
            return con.SalaryReports.Include(x => x.Employee).ThenInclude(e => e.Department).ToList();
        }
        public List<SalaryReport> GetAllReportsForEmp (int empid)
        {
            return con.SalaryReports.Include(s => s.Employee).ThenInclude(e=>e.Department).Where(x=>x.EmployeeId == empid).ToList();
        }
    }
}
