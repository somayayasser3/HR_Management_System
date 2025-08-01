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
                .Include(l => l.Employee).Include(l => l.LeaveType).ToList();
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

        public bool FoundLeaveRequest(int id)
        {
             int x = con.LeaveRequests.Where(l => l.EmployeeId == id && l.Status == "Pending").ToList().Count() ;
            return x > 0;
        }

        public int LeaveRequestTypeApprovedCountForEmployee(int id,int leaveID)
        {
            return con.LeaveRequests.Where(r => r.EmployeeId == id && r.Status == "approved" && r.LeaveTypeId == leaveID ).Sum(r => r.RequestDaysCount);
        }
    }
}
