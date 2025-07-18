using AutoMapper;
using HrManagementSystem.DTOs.OfficialHoliday;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [EndpointSummary("All Official Holidays")]
        public IActionResult GetAllOfficialHolidays()
        {
            var holidays = unit.OfficialHolidayRepo.getAll();
            var holidayDTOs = mapper.Map<List<OfficialHolidayDisplayDTO>>(holidays);
            return Ok(holidayDTOs);
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get specific  Official Holiday")]
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
        public IActionResult AddOfficialHoliday( EditOfficalHolidayDTO holidayDisplayDTO)
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
        public IActionResult EditOfficialHoliday(int id, EditOfficalHolidayDTO officialHolidayDisplay)
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
        [EndpointSummary("Delete an existing Official Holiday")]
        public IActionResult DeleteOfficialHoliday(int id)
        {

            unit.OfficialHolidayRepo.Delete(id);
            unit.Save();

            return Ok();
        }
    }
}
