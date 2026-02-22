using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : Controller
    {
        private readonly IRepository<Master> _Repository;
        private readonly IRepository<ServiceAppointment> _appointmentRepo;

        public MasterController(IRepository<Master> repository, IRepository<ServiceAppointment> appointmentRepo)
        {
            _Repository = repository;
            _appointmentRepo = appointmentRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var masters = await _Repository.GetAllAsync();
            return Ok(masters);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Master master)
        {
            await _Repository.CreateAsync(master);
            return CreatedAtAction(nameof(GetAll), new { id = master.Id }, master);
        }

        [HttpGet("{id}/busy-slots")]
        public async Task<IActionResult> GetBusySlots(Guid id, [FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
                return BadRequest("Невірний формат дати");

            var allAppointments = await _appointmentRepo.GetAllAsync();
            if (allAppointments == null) return Ok(new List<string>());

            var busySlots = allAppointments
                .Where(a => a.MasterId == id)
                .Where(a => a.Start_date.Date == selectedDate.Date)
                .Select(a => a.Start_date.ToString("HH:mm"))
                .Distinct()
                .ToList();

            return Ok(busySlots);
        }
    }
}
