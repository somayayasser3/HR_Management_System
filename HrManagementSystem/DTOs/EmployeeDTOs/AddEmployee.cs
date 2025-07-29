using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.Employee
{
    public class AddEmployee
    {

        public string FullName { get; set; }
        public string Address { get; set; }
        [RegularExpression(@"^\+201[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        [MaxLength(13)]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [MaxLength(14)]
        public string NationalId { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public int DepartmentId { get; set; }

        public IFormFile Image { get; set; }


    }
}
