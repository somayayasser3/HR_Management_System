using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.Models
{
    public class UserGroup
    {
        [Key]
        public int GroupId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<GroupPermission> GroupPermissions { get; set; }
       // public virtual ICollection<User> Users { get; set; }
    }
}
