using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class LeaveTypeRepository : GenericRepo<LeaveType>
    {
        public LeaveTypeRepository(HRContext context) : base(context)
        {
        }
    }
}
