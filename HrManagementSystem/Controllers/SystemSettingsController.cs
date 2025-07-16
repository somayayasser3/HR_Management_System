using AutoMapper;
using HrManagementSystem.DTOs.SystemSettings;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult GetAllSystemSettings()
        {
            var settings = unit.SystemSettingRepo.getAll();
            var settingsDto = mapper.Map<List<DisplaySystemSettingsDTO>>(settings);
            return Ok(settingsDto);
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get specific system settings by ID ")]
        public IActionResult GetSystemSettingsById(int id)
        {
            var setting = unit.SystemSettingRepo.getByID(id);
            if (setting == null)
                return NotFound();

            var settingDto = mapper.Map<DisplaySystemSettingsDTO>(setting);
            return Ok(settingDto);
        }

        [HttpPost]
        [EndpointSummary("Add new System settings")]
        public IActionResult AddSystemSetting(DisplaySystemSettingsDTO dto)
        {
            var setting = mapper.Map<SystemSetting>(dto);
            setting.UpdatedAt = DateTime.Now;

            unit.SystemSettingRepo.Add(setting);
            unit.Save();
            return Ok(setting);
        }

        [HttpPut("{id}")]
        [EndpointSummary("Edit existing setting by ID ")]
        public IActionResult EditSystemSetting(int id, DisplaySystemSettingsDTO EditedDTO)
        {
            var existingSetting = unit.SystemSettingRepo.getByID(id);
            if (existingSetting == null)
                return NotFound();

            mapper.Map(EditedDTO, existingSetting);
            existingSetting.UpdatedAt = DateTime.Now;

            unit.SystemSettingRepo.Update(existingSetting);
            unit.Save();

            return Ok();
        }

        [HttpDelete("{id}")]
        [EndpointSummary("Delete Existing settings")]
        public IActionResult DeleteSystemSettings(int id)
        {
            if (id == null)
                return NotFound();

            unit.SystemSettingRepo.Delete(id);
            unit.Save();

            return Ok();
        }


    }
}
