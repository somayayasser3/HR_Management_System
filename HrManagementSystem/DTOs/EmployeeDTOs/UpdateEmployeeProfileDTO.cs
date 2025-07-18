using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.EmployeeDTOs
{
    public class UpdateEmployeeProfileDTO
    {
        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }
    }
}
