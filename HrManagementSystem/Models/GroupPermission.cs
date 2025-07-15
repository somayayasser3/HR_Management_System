using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagementSystem.Models
{
    public class GroupPermission
    {
        public int GroupId { get; set; }
        public int PermissionId { get; set; }

        [ForeignKey("GroupId")]
        public virtual UserGroup UserGroup { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }
    }
}
