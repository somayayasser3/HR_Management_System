using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class AttendanceRepository : GenericRepo<Attendance>
    {
        public AttendanceRepository(HRContext context) : base(context){}

        public List<Attendance>GetAttendanceWithEmployees()
        {
            return con.Attendances.Include(x => x.Employee).ThenInclude(x=>x.Department).ToList();
        }
        public List<Attendance> GetAttendanceForEmployee(int id)
        {
            return con.Attendances.Include(x => x.Employee).ThenInclude(x => x.Department).Where(x=>x.EmployeeId==id).ToList();
        }
        public Attendance GetSingleAttendanceForEmployee(int id)
        {
            return con.Attendances.Include(x => x.Employee).ThenInclude(x => x.Department).FirstOrDefault(x=>x.AttendanceId==id);
        }
        public Attendance GetSingleAttendanceForEmployeeByEmployeeIdandDate(int id, DateTime date)
        {
            return con.Attendances.Where(x => x.EmployeeId == id && x.AttendanceDate == date).FirstOrDefault();
        }
    }
}
