using AutoMapper;
using HrManagementSystem.DTOs.WorkingTaskDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkingTaskController : ControllerBase
    {
        IMapper mapper;
        UnitOfWork unit;
        public WorkingTaskController(UnitOfWork u, IMapper m)
        {
            unit = u;
            mapper = m;
        }

        [HttpPost]
        [EndpointSummary("Adding new task for employee")]
        public async Task<IActionResult> AddNewWorkingTask(AddWorkingTaskDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Empty task" });
            if (unit.EmployeeRepo.getByID(dto.EmployeeId) == null)
                return BadRequest(new { message = "Employee Not found" });

            if (dto.DueDate < DateTime.Now.Date)
                return BadRequest(new { message = "Can't add past date tasks" });
            try
            {

                WorkingTask workingTask = mapper.Map<WorkingTask>(dto);
                workingTask.CreatedAt = DateTime.Now;
                workingTask.UpdatedAt = DateTime.Now;
                workingTask.Status = "Pending";
                unit.WorkingTaskRepo.Add(workingTask);
                unit.Save();
                DisplayWorkingTaskDTO WKT = mapper.Map<DisplayWorkingTaskDTO>(await unit.WorkingTaskRepo.SingleWorkingTask(workingTask.Id));
                return Ok(WKT);
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }
        [HttpGet("All")]
        [EndpointSummary("Get All Tasks")]
        public async Task<IActionResult> GetWorkingTasks()
        {

            List<DisplayWorkingTaskDTO> AllTasks = mapper.Map<List<DisplayWorkingTaskDTO>>(await unit.WorkingTaskRepo.GetAllTasksWithEmployee());
            if (AllTasks == null)
            {
                return BadRequest(new { message = "No tasks" });
            }
            return Ok(AllTasks);
        }

        [HttpGet("Task")]
        [EndpointSummary("Get Specific Task")]
        public async Task<IActionResult> GetTask(int id)
        {
            DisplayWorkingTaskDTO task = mapper.Map<DisplayWorkingTaskDTO>(await unit.WorkingTaskRepo.GetTaskByIdWithEmployee(id));
            if (task == null)
            {
                return BadRequest(new { message = "No such task" });
            }
            return Ok(task);
        }

        [HttpGet("employee/tasks/{empId}")]
        [EndpointSummary("Get Employee Tasks")]
        public async Task<IActionResult> GetAllEmployeeTasks(int empId)
        {
            if (unit.EmployeeRepo.getByID(empId) == null) return BadRequest(new { message = "No such employee" });
            List<DisplayWorkingTaskDTO> AllEmpTasks = mapper.Map<List<DisplayWorkingTaskDTO>>(await unit.WorkingTaskRepo.getEmployeeAllTasks(empId));
            if (AllEmpTasks == null)
            {
                return BadRequest(new { message = "Employee has no tasks" });
            }
            return Ok(AllEmpTasks);
        }

        [HttpPut("employee")]
        [EndpointSummary("Updating existing task")]
        public async Task<IActionResult> EmployeeUpdateTask(EmployeeUpdatesTaskDTO dto)
        {
            WorkingTask task = unit.WorkingTaskRepo.getByID(dto.TaskId);
            if (task == null)
                return BadRequest(new { message = "No such task" });
            if (DateTime.Now.Date > task.DueDate.Date)
                dto.Status = "Late";
            try
            {

                task.Status = dto.Status;
                task.UpdatedAt = DateTime.Now;
                unit.WorkingTaskRepo.Update(task);
                unit.Save();
                return Ok(new { status = task.Status });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }

        [HttpPut("admin")]
        public async Task<IActionResult> AdminUpdatesTask(AdminUpdatesTaskDTO dto)
        {
            WorkingTask task = unit.WorkingTaskRepo.getByID(dto.Id);
            if (task == null)
                return BadRequest(new { message = "No such task" });
            if (unit.EmployeeRepo.getByID(dto.EmployeeId) == null)
                return NotFound(new { message = "Employee not found" });
            try
            {

                task = mapper.Map(dto, task);
                task.UpdatedAt = DateTime.Now;
                task.Status = dto.Status;
                unit.WorkingTaskRepo.Update(task);
                unit.Save();
                return Ok(new { message = "Task updated" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Try again" });
            }
        }
    }
}
