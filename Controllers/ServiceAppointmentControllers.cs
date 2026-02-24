using Application.DTOs;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceAppointmentControllers : Controller
    {
        private readonly IRepository<ServiceAppointment> _Repository;
        private readonly IRepository<Service> _ServiceRepo;
        private readonly IRepository<Master> _MasterRepo;
        public readonly IRepository<Client> _ClientRepo;

        public ServiceAppointmentControllers(IRepository<ServiceAppointment> repository, IRepository<Service> serviceRepo, IRepository<Master> masterRepo, IRepository<Client> clientRepo ) 
        {
            _Repository = repository;
            _ServiceRepo = serviceRepo;
            _MasterRepo = masterRepo;
            _ClientRepo = clientRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _Repository.GetAllAsync();
            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            var client = (await _ClientRepo.GetAllAsync())
                         .FirstOrDefault(c => c.Phone == dto.Phone);

            Guid finalClientId;

            if (client == null)
            {
                var newClient = new Client
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email
                };
                await _ClientRepo.CreateAsync(newClient);
                finalClientId = newClient.Id;
            }
            else
            {
                finalClientId = client.Id;
            }

            var appointment = new ServiceAppointment
            {
                Id = Guid.NewGuid(),
                ClientId = finalClientId, 
                MasterId = dto.MasterId,
                ServiceId = dto.ServiceId,
                Start_date = dto.Start_date,
                Status = AppointmentStatus.IN_PROGRESS,
                TotalPrice = 0 
            };

            await _Repository.CreateAsync(appointment);
            return Ok(new { success = true, clientId = finalClientId });
        }

        [Authorize]
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _Repository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.ClientId.ToString() != userId)
            {
                return Forbid();
            }

            booking.Status = AppointmentStatus.CANCELLED;
            await _Repository.UpdateAsync(id, booking);
            return NoContent();
        }

        [Authorize]
        [HttpGet("my-bookings/{clientId}")]
        public async Task<IActionResult> GetMyBookings(string clientId)
        {
            if (!Guid.TryParse(clientId, out var clientGuid)) return BadRequest();

            var appointments = await _Repository.GetAllAsync();
            var services = await _ServiceRepo.GetAllAsync();
            var masters = await _MasterRepo.GetAllAsync();

            var userBookings = appointments
              .Where(a => a.ClientId == clientGuid)
              .OrderByDescending(a => a.Start_date)
              .Select(a => {
                var service = services.FirstOrDefault(s => s.Id == a.ServiceId);
                var master = masters.FirstOrDefault(m => m.Id == a.MasterId);

                return new
                {
                    id = a.Id.ToString(),
                    bookingId = "BS-" + a.Id.ToString().Substring(0, 8).ToUpper(),
                    serviceName = service?.Title ?? "Unknown Service", 
                    masterName = master?.Name ?? "Unknown Master",   
                    date = a.Start_date.ToString("yyyy-MM-dd"),
                    time = a.Start_date.ToString("HH:mm"),
                    status = a.Status.ToString(),
                    price = service?.ServicePrice ?? 0 
                };
            }).ToList();

            return Ok(userBookings);
        }
    }
}

