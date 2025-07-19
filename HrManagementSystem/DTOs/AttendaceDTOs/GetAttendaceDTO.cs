using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class GetAttendaceDTO
    {
        public int AttendanceId { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal DelayHours { get; set; }
    }
}
