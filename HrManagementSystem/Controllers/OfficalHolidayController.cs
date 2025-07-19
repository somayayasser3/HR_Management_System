using AutoMapper;
using HrManagementSystem.DTOs.OfficialHoliday;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OfficalHolidayController : Controller
    {
        UnitOfWork unit;
        IMapper mapper;
        public OfficalHolidayController(IMapper Map, UnitOfWork unitofWork)
        {
            this.mapper = Map;
            this.unit = unitofWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR,Employee")]
        [EndpointSummary("All Official Holidays")]
        public IActionResult GetAllOfficialHolidays()
        {
            var holidays = unit.OfficialHolidayRepo.getAll();
            var holidayDTOs = mapper.Map<List<OfficialHolidayDisplayDTO>>(holidays);
            return Ok(holidayDTOs);
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get specific  Official Holiday")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public IActionResult GetOfficialHolidayById(int id)
        {
            var holiday = unit.OfficialHolidayRepo.getByID(id);
            if (holiday == null)
                return NotFound();

            var holidayDTO = mapper.Map<OfficialHolidayDisplayDTO>(holiday);
            return Ok(holidayDTO);
        }

        [HttpPost]
        [EndpointSummary("Add Official Holiday")]
        [Authorize(Roles = "HR")]
        public IActionResult AddOfficialHoliday( OfficialHolidayDisplayDTO holidayDisplayDTO)
        {
            
            var holiday = mapper.Map<OfficialHoliday>(holidayDisplayDTO);
            holiday.CreatedAt = DateTime.Now;
            holiday.UpdatedAt = DateTime.Now;

            unit.OfficialHolidayRepo.Add(holiday);
            unit.Save();

            return Ok(holiday);
        }

        [HttpPut("{id}")]
        [EndpointSummary("Edit an existing Official Holiday")]
        [Authorize(Roles = "HR")]
        public IActionResult EditOfficialHoliday(int id, OfficialHolidayDisplayDTO officialHolidayDisplay)
        {
            var existingHoliday = unit.OfficialHolidayRepo.getByID(id);
            if (existingHoliday == null)
                return NotFound();

            mapper.Map(officialHolidayDisplay, existingHoliday);
            existingHoliday.UpdatedAt = DateTime.Now;

            unit.OfficialHolidayRepo.Update(existingHoliday);
            unit.Save();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HR")]
        [EndpointSummary("Delete an existing Official Holiday")]
        public IActionResult DeleteOfficialHoliday(int id)
        {

            unit.OfficialHolidayRepo.Delete(id);
            unit.Save();

            return Ok();
        }
    }
}
