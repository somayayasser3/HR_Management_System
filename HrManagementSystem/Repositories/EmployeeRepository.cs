using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class EmployeeRepository : GenericRepo<Employee>
    {
        public EmployeeRepository(HRContext context) : base(context){}


        public List<Employee> GetEmployeesandDepartment()
        {
            return con.Employees.Include(e => e.Department).Include(e=>e.User).ToList();
        }


        public Employee GetEmployeeWithUserID(int id)
        {
            return con.Employees.Include(e => e.Department).Include(e => e.User).Include(e=>e.Attendances).Where(e => e.UserId == id).FirstOrDefault();
        }
        public Employee GetEmployeeWithDeptBYID (int id)
        {
            return con.Employees.Include(e => e.Department).Include(e=> e.User).Where(e => e.EmployeeId == id).FirstOrDefault();
        }

        public Employee GetEmployeeByUserId(int id)
        {

            return con.Employees.Include(e => e.Department).Include(e => e.User).Include(e => e.Attendances).Where(e => e.UserId == id).FirstOrDefault();
        }
        
        public Employee GetAnotherExistingByPhoneNumber(int id , string phone)
        {
            return con.Employees.Where(x => x.EmployeeId != id && x.PhoneNumber == phone).FirstOrDefault();
        }
        public Employee GetAnotherExistingByNationalID(int id , string nat)
        {
            return con.Employees.Where(x => x.EmployeeId != id && x.NationalId== nat).FirstOrDefault();
        }
        public Employee GetExistingByPhoneNumber(string phone)
        {
            return con.Employees.Where(x=>x.PhoneNumber == phone).FirstOrDefault();
        }
        public Employee GetExistingByNationalID( string nat)
        {
            return con.Employees.Where(x => x.NationalId== nat).FirstOrDefault();
        }
        public List<Employee> GetEmployeesByDeptId( int id)
        {
            return con.Employees.Where(x => x.DepartmentId == id).ToList();
        }

        public TimeSpan GetStartTimeForEmployee(int id)
        {
            return con.Employees.Where(x => x.EmployeeId == id).Select(x => x.WorkStartTime).FirstOrDefault();
        }
        public TimeSpan GetEndTimeForEmployee(int id)
        {
            return con.Employees.Where(x => x.EmployeeId == id).Select(x => x.WorkEndTime).FirstOrDefault();
        }


    }
}
