using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.Employee
{
    public class DisplayEmployeeData
    {

        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalId { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }

        public DateTime WorkStartTime { get; set; }
        public DateTime WorkEndTime { get; set; }

       
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 

    }
}
