using HrManagementSystem.Models;

namespace HrManagementSystem.Repositories
{
    public class AttendanceRepository : GenericRepo<Attendance>
    {
        public AttendanceRepository(HRContext context) : base(context){}
    }
}
