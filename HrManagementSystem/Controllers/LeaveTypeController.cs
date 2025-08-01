using AutoMapper;
using HrManagementSystem.DTOs.LeaveType;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypeController : ControllerBase
    {
        IMapper mapper;
        UnitOfWork unit;
        public LeaveTypeController(UnitOfWork u , IMapper m )
        {
            mapper = m;
            unit = u;
        }

        [HttpGet]
        public IActionResult GetAllLeaveTypes()
        {
            var types = mapper.Map<List<GetLeaveTypeDTO>>(unit.LeaveTypeRepo.getAll());           
            return Ok(types);
        }

        [HttpPost]
        public IActionResult AddLeaveType(AddLeaveTypeDTO dto)
        {
            var leave = mapper.Map<LeaveType>(dto);
            unit.LeaveTypeRepo.Add(leave);
            unit.Save();
            return Ok(new { message = "Leave type added" });
        }


        [HttpDelete]
        public IActionResult DeleteLeaveType(int id)
        {

            var leave = unit.LeaveTypeRepo.getByID(id);
            if (leave == null) 
            {
                return BadRequest(new { message = "No such leave type" });            
            }
            unit.LeaveTypeRepo.Delete(id);
            unit.Save();
            return Ok(new { message = "Leave type removed" });
        }

        [HttpPut]
        public IActionResult UpdateLeaveType(GetLeaveTypeDTO dto)
        {

            var leave = unit.LeaveTypeRepo.getByID(dto.Id);
            if (leave == null) 
            {
                return BadRequest(new { message = "No such leave type" });            
            }
            leave = mapper.Map(dto,leave);
            unit.LeaveTypeRepo.Update(leave);
            unit.Save();
            return Ok(new { message = "Leave type updated" });
        }

    }
}
