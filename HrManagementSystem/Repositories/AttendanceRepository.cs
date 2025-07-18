using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class AttendanceRepository : GenericRepo<Attendance>
    {
        public AttendanceRepository(HRContext context) : base(context){}

        public List<Attendance>GetAttendanceWithEmployees()
        {
            return con.Attendances.Include(x => x.Employee).ToList();
        }
        public List<Attendance> GetAttendanceForEmployee(int id)
        {
            return con.Attendances.Include(x => x.Employee).Where(x=>x.EmployeeId==id).ToList();
        }
    }
}
