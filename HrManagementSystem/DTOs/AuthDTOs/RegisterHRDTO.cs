using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AuthDTOs
{
    public class RegisterHRDTO
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
    }
}
