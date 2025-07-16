using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class UserGroupRepository : GenericRepo<UserGroup>
    {
        public UserGroupRepository(HRContext context) : base(context){}
    }
}
