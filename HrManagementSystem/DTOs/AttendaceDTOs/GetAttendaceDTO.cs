using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class GetAttendaceDTO
    {
        public int AttendanceId { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }     
        public decimal OvertimeHours { get; set; }
        public decimal DelayHours { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
