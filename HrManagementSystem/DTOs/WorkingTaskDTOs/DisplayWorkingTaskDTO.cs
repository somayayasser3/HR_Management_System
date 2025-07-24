using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.DTOs.WorkingTaskDTOs
{
    public class DisplayWorkingTaskDTO
    {
        public int TaskId { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string DepartmentName { get; set; }

    }
}
