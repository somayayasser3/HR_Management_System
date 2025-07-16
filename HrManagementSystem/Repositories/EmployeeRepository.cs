using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class EmployeeRepository : GenericRepo<Employee>
    {
        public EmployeeRepository(HRContext context) : base(context){}
    }
}
