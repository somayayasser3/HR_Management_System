//using HrManagementSystem.Models;
//using Microsoft.EntityFrameworkCore;

//namespace HrManagementSystem.Repositories
//{
//    public class EmployeeLeaveBalanceRepository : GenericRepo<EmployeeLeaveBalance>
//    {
//        public EmployeeLeaveBalanceRepository(Models.HRContext context) : base(context)
//        {
//        }

//        public EmployeeLeaveBalance EmpBalanceByID(int id)
//        {
//            return con.EmployeeLeaveBalances
//                .Include(e => e.Employee)
//                .FirstOrDefault(e => e.EmployeeId == id);
//        }
//    }
//}
