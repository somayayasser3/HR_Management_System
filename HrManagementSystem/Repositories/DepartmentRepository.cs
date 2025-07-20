using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class DepartmentRepository : GenericRepo<Department>
    {
        public DepartmentRepository(HRContext context) : base(context){}
    }


}
