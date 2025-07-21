using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class EmployeeRepository : GenericRepo<Employee>
    {
        public EmployeeRepository(HRContext context) : base(context){}


        public List<Employee> GetEmployeesandDepartment()
        {
            return con.Employees.Include(e => e.Department).Include(e=>e.User).Include(e=>e.LeaveBalance).ToList();
        }
        
        public Employee GetEmployeeWithDeptBYID (int id)
        {
            return con.Employees.Include(e => e.Department).Include(e=> e.User).Include(e => e.LeaveBalance).Where(e => e.EmployeeId == id).FirstOrDefault();
        }
    }
}
