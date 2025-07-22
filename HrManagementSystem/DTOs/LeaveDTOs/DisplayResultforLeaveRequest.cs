namespace HrManagementSystem.DTOs.LeaveDTOs
{
    public class DisplayResultforLeaveRequest
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveTypeName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }

    }
}
