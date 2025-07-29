using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.Employee
{
    public class DisplayEmployeeData
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalId { get; set; }
        public string Gender { get; set; }
        public DateTime HireDate { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public decimal Salary { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentId { get; set; }


    }
}
