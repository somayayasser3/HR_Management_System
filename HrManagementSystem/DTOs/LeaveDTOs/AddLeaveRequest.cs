namespace HrManagementSystem.DTOs.LeaveDTOs
{
    public class AddLeaveRequest
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string Reason { get; set; }
    }
}
