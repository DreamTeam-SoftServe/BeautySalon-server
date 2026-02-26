using Application.DTOs; 
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : Controller
    {
        private readonly IRepository<Master> _Repository;
        private readonly IRepository<ServiceAppointment> _appointmentRepo;
        private readonly IRepository<Client> _clientRepository;


        public MasterController(IRepository<Master> repository, IRepository<ServiceAppointment> appointmentRepo, IRepository<Client> clientRepository)
        {
            _Repository = repository;
            _appointmentRepo = appointmentRepo;
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var masters = await _Repository.GetAllAsync();
            return Ok(masters);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMasterDto dto)
        {
            var masterId = Guid.NewGuid();
            var master = new Master
            {
                Id = masterId,
                Name = dto.Name,
                Phone = dto.Phone,
                Gender = (GenderType)dto.Gender,
                Rating = 5.0,
                PricePersent = dto.PricePersent > 0 ? dto.PricePersent : 40,
                ProfLevel = (ProficiencyType)dto.ProfLevel,
                Specialization = (ServiceType)dto.Specialization,
                ImageUrl = dto.ImageUrl,

                prices = new Dictionary<ServiceType, int>
                {
                    { (ServiceType)dto.Specialization, 500 }
                },

                Sсhedule = new Schedule
                {
                    WorkDays = new List<WorkDay>(),
                    Appointments = new List<ServiceAppointment>()
                }
            };

            await _Repository.CreateAsync(master);

            if (!string.IsNullOrEmpty(dto.Email) && !string.IsNullOrEmpty(dto.Password))
            {
                var clientAccount = new Client
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Role = "Master",
                    MasterProfileId = masterId,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                };

                await _clientRepository.CreateAsync(clientAccount);
            }

            return Ok(master);
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaster(Guid id)
        {
            try
            {
                var master = await _Repository.GetByIdAsync(id);

                if (master == null)
                {
                    return NotFound(new { message = "No craftsmen found" });
                }

                await _Repository.DeleteAsync(id);

                return Ok(new { message = "Master successfully removed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateMasterDto dto)
        {
            var master = await _Repository.GetByIdAsync(id);
            if (master == null)
                return NotFound(new { message = "Master not found" });

            master.Name = dto.Name;
            master.Phone = dto.Phone;
            master.Gender = (GenderType)dto.Gender;
            master.PricePersent = dto.PricePersent > 0 ? dto.PricePersent : 40;
            master.ProfLevel = (ProficiencyType)dto.ProfLevel;
            master.Specialization = (ServiceType)dto.Specialization;
            master.ImageUrl = dto.ImageUrl;

            if (!string.IsNullOrEmpty(dto.Experience))
                master.Experience = dto.Experience;

            await _Repository.UpdateAsync(id, master);
            return Ok(master);
        }
    }
}