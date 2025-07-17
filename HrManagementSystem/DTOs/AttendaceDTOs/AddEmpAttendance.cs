using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class AddEmpAttendance
    {

        public int EmployeeId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public decimal? OvertimeHours { get; set; }

        public decimal? DelayHours { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
