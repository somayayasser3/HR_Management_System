using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class OfficialHolidayRepository : GenericRepo<OfficialHoliday>
    {
        public OfficialHolidayRepository(HRContext context) : base(context){}
    }
}
