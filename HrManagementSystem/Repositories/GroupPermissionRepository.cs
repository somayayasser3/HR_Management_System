using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class GroupPermissionRepository : GenericRepo<GroupPermission>
    {
        public GroupPermissionRepository(HRContext context) : base(context){}
    }
}
