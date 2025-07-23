using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class LeaveRepository : GenericRepo<LeaveRequest>
    {
        public LeaveRepository(HRContext context) : base(context)
        {
        }

        public List<LeaveRequest> GetAllRequests()
        {
            return con.LeaveRequests
                .Include(l => l.Employee).Include(l=> l.Employee.LeaveBalance)
                .Include(l => l.LeaveType)
                .ToList();
        }

        public List<LeaveType> GetAllTypes()
        {
            return con.LeaveTypes.ToList();
        }
        public LeaveRequest GetRequestByID(int id)
        {
            return con.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType).Where(l => l.Id == id)
                .FirstOrDefault();
        }

        public LeaveType GetLeaveTypeofReq(int id )
        {
            return con.LeaveTypes.Where(t => t.Id == id).FirstOrDefault();
        }
    }
}
