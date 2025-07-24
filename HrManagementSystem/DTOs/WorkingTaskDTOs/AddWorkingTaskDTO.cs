using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.WorkingTaskDTOs
{
    public class AddWorkingTaskDTO
    {
        public int EmployeeId { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
    }
}
