using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.DepartmentsDTOs
{
    public class GetDepartmentsDTO
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public string Description { get; set; }
    }
}
