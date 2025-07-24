using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class UpdateEmployeeAttendance
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        [DefaultValue(typeof(TimeSpan), "16:00:00")]
        public TimeSpan? CheckOutTime { get; set; }

        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }


        //public decimal? OvertimeHours { get; set; }

        //public decimal? DelayHours { get; set; }

        //public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
