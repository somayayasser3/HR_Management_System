using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class AddEmpAttendance
    {
        [Required]
        public int EmployeeId { get; set; }
        //[Required]
        //public DateTime AttendanceDate { get; set; }
        [Required]
        [DefaultValue(typeof(TimeSpan), "8:00:00")]
        public TimeSpan CheckInTime { get; set; } 
        //[Required]
        //[DefaultValue(typeof(TimeSpan), "16:00:00")]
        //public TimeSpan? CheckOutTime { get; set; }

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
