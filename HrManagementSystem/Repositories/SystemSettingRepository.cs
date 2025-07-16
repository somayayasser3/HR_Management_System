using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class SystemSettingRepository : GenericRepo<SystemSetting>
    {
        public SystemSettingRepository(HRContext context) : base(context){}
    }
}
