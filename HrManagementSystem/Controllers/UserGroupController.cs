using AutoMapper;
using HrManagementSystem.DTOs.UserGroupsDTOs;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        UnitOfWork unit;
        IMapper mapper;
        public UserGroupController(UnitOfWork u , IMapper m)
        {
            unit = u;
            mapper = m;
            
        }

        [HttpGet]
        public IActionResult GetAllUserGroups()
        {
            List<GetUserGroupsDTO> groups = mapper.Map<List<GetUserGroupsDTO>>(unit.UserGroupRepo.getAll());
            return Ok(groups);
        }
    }
}
