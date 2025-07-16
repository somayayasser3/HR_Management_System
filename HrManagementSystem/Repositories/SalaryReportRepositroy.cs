using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class SalaryReportRepositroy : GenericRepo<SalaryReportRepositroy>
    {
        public SalaryReportRepositroy(HRContext context) : base(context){}
    }
}
