using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.DTOs.WorkingTaskDTOs
{
    public class DisplayWorkingTaskDTO
    {

        public string EmployeeName { get; set; }

        public string Description { get; set; }
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }

    }
}
