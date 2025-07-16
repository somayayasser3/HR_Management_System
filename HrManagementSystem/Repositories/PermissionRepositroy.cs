using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class PermissionRepositroy:GenericRepo<Permission>
    {
        public PermissionRepositroy(HRContext con):base(con) {}
    }
}
