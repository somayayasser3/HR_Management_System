using AutoMapper;
using HrManagementSystem.DTOs.SystemSettings;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemSettingsController : Controller
    {


        UnitOfWork unit;
        IMapper mapper;
        public SystemSettingsController(IMapper Map, UnitOfWork unitofWork)
        {
            this.mapper = Map;
            this.unit = unitofWork;
        }

        [HttpGet]
        [EndpointSummary("Get All System Settings")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public IActionResult GetAllSystemSettings()
        {
            var settings = unit.SystemSettingRepo.getAll().FirstOrDefault();
            if (settings == null)
            {
                return NotFound(new { message = "No system settings found." });
            }           
            return Ok(settings);
        }



        //[HttpPost]
        //[EndpointSummary("Add new System settings")]
        //[Authorize(Roles = "Admin,HR")]
        //public IActionResult AddSystemSetting(DisplaySystemSettingsDTO dto)
        //{
        //    var setting = mapper.Map<SystemSetting>(dto);
        //    setting.UpdatedAt = DateTime.Now;

        //    unit.SystemSettingRepo.Add(setting);
        //    unit.Save();
        //    return Ok(new { message = "System settings Added "});
        //}

        [HttpPut("{id}")]
        [EndpointSummary("Edit existing setting by ID ")]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult EditSystemSetting(int id, UpdateSystemSettingsDTO EditedDTO)
        {
            var existingSetting = unit.SystemSettingRepo.getByID(id);
            if (existingSetting == null)
                return NotFound(new { message = "System setting not found." });

            if(EditedDTO.HoursRate!="Money" && EditedDTO.HoursRate != "Hours")
                return BadRequest(new {message = "Hours rate not correct"});
            
            mapper.Map(EditedDTO, existingSetting);
            existingSetting.UpdatedAt = DateTime.Now;
            unit.SystemSettingRepo.Update(existingSetting);
            unit.Save();
            return Ok(existingSetting);
        }

        //[HttpDelete("{id}")]
        //[EndpointSummary("Delete Existing settings")]
        //[Authorize(Roles = "Admin,HR")]
        //public IActionResult DeleteSystemSettings(int id)
        //{
        //    if (id == null)
        //        return NotFound(new {message = "System setting not found." });

        //    unit.SystemSettingRepo.Delete(id);
        //    unit.Save();

        //    return Ok(new { message = "System setting deleted successfully." });
        //}


    }
}
