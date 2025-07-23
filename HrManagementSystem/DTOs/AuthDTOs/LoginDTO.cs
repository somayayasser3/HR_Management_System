using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AuthDTOs
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public IFormFile Image { get; set; }
    }
}
