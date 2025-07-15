using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.Models
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string ModuleName { get; set; }

        public string ActionType { get; set; }

        // Navigation properties
        public virtual ICollection<GroupPermission> GroupPermissions { get; set; }
    }
}
