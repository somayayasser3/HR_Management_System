using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class AddEmpAttendance
    {

        public int EmployeeId { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public decimal? OvertimeHours { get; set; }

        public decimal? DelayHours { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime AttendanceDate { get; set; } = DateTime.UtcNow.Date; // Default to today's date
    }
}
