using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.Models
{

    public enum UserRole
    {
        Admin = 1,
        HR = 2,
        Employee = 3
    }
    public class User : IdentityUser<int>
    {
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(13)]
        public string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public UserRole Role { get; set; }

        //[ForeignKey("UserGroup")]
        //public int GroupID { get; set; }
        // public virtual UserGroup UserGroup { get; set; }

        public virtual Employee Employee { get; set; }  // relation with Employee model
    }
}
