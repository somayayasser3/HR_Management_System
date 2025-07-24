using AutoMapper;
using HrManagementSystem.DTOs.DepartmentsDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,HR")]
    public class DepartmentController : ControllerBase
    {
        UnitOfWork unit;
        IMapper mapper;
        public DepartmentController(UnitOfWork u, IMapper m)
        {
            unit = u;
            mapper = m;
        }
        //D
        [HttpGet]
        [EndpointSummary("Get all departments in system")]
        public ActionResult GetAllDepartments()
        {
            List<GetDepartmentsDTO> allDepts = mapper.Map<List<GetDepartmentsDTO>>(unit.DepartmentRepo.getAll());
            return Ok(allDepts);
        }
        [HttpPost]
        [EndpointSummary("Adds new deaprtment")]
        public IActionResult AddNewDepartment(AddNewDepartmentDTO newDept)
        {
            Department newDepartment = mapper.Map<Department>(newDept);
            newDepartment.CreatedAt = DateTime.Now;
            newDepartment.UpdatedAt = DateTime.Now;
            unit.DepartmentRepo.Add(newDepartment);
            unit.Save();
            return Ok(newDepartment);
        }
        [HttpPut]
        [EndpointSummary("Updates Department Data")]
        public IActionResult UpdateDepartment(UpdateExistingDepartmentDTO deptToUpdate)
        {
            Department updatedDept = unit.DepartmentRepo.getByID(deptToUpdate.DepartmentId);
            if (updatedDept == null) {
                return BadRequest(new { message = "No such department" });
            }
            //updatedDept = mapper.Map<Department>(deptToUpdate);
            mapper.Map(deptToUpdate, updatedDept);
            updatedDept.UpdatedAt = DateTime.Now;
            unit.DepartmentRepo.Update(updatedDept);
            unit.Save();
            return Ok(updatedDept);
        }

        [HttpDelete("{id}")]
        [EndpointSummary("Deletes Department")]
        public IActionResult DeleteDepartment(int id)
        {
            Department deletedDept= unit.DepartmentRepo.getByID(id);
            if (deletedDept == null) {
                return BadRequest(new { message = "No such department" } );
            }
            unit.DepartmentRepo.Delete(id);
            unit.Save();
            return Ok();
        }

    }
}
