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
        
        public Employee GetEmployeeWithDeptBYID (int id)
        {
            return con.Employees.Include(e => e.Department).Include(e=> e.User).Where(e => e.EmployeeId == id).FirstOrDefault();
        }

        public Employee GetEmployeeByUserId(int id)
        {

            return con.Employees.Where(E => E.UserId == id).FirstOrDefault(); 
        }


    }
}
