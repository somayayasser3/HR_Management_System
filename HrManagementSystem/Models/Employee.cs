using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.Models
{
    public class Employee
    {

        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string NationalId { get; set; }

        [MaxLength(50)]
        public string Gender { get; set; }

        public DateTime HireDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key to User
        [Required]
        public int UserId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        public string ImagePath { get; set; }

        //public EmployeeLeaveBalance LeaveBalance { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}

