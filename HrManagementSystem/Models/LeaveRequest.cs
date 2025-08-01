using Microsoft.AspNetCore.Identity;

namespace HrManagementSystem.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int RequestDaysCount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
    }

}
