using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace HrManagementSystem.Repositories
{
    public class WorkingTaskRepository : GenericRepo<WorkingTask>
    {
        public WorkingTaskRepository(HRContext context) : base(context)
        {
        }

        public async Task<List<WorkingTask>> getEmployeeAllTasks(int id)
        {
            return await con.WorkingTasks.Where(x=>x.EmployeeId== id).Include(x=>x.Employee).ThenInclude(e => e.Department).ToListAsync();   
        }
        public async Task<List<WorkingTask>> GetAllTasksWithEmployee()
        {
            return await con.WorkingTasks.Include(x=>x.Employee).ThenInclude(e=>e.Department).ToListAsync();   
        }
        public async Task<WorkingTask> GetTaskByIdWithEmployee(int id)
        {
            return await con.WorkingTasks.Where(t=>t.Id == id).Include(x=>x.Employee).FirstOrDefaultAsync();   
        }
        public async Task<WorkingTask> SingleWorkingTask(int id)
        {
            return await con.WorkingTasks.Where(t=>t.Id == id).Include(x=>x.Employee).ThenInclude(d=>d.Department).FirstOrDefaultAsync();   
        }




    }
}
