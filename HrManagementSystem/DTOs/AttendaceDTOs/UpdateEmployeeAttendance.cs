using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class UpdateEmployeeAttendance
    {
        [Required]
        public int AttendanceId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public DateTime AttendanceDate { get; set; }
        [Required]
        [DefaultValue(typeof(TimeSpan), "8:00:00")]
        public TimeSpan CheckInTime { get; set; }
        [Required]
        [DefaultValue(typeof(TimeSpan), "16:00:00")]
        public TimeSpan? CheckOutTime { get; set; }

        //public decimal? OvertimeHours { get; set; }

        //public decimal? DelayHours { get; set; }

        //public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
