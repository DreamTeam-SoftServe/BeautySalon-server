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
            Guid finalClientId;

            if (dto.ClientId.HasValue && dto.ClientId.Value != Guid.Empty)
            {
                finalClientId = dto.ClientId.Value;
            }
            else
            {
                var client = (await _ClientRepo.GetAllAsync())
                             .FirstOrDefault(c => c.Phone == dto.Phone);

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
            }

            var parsedDate = DateTime.Parse(dto.Start_date);
            var exactTime = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            var appointment = new ServiceAppointment
            {
                Id = Guid.NewGuid(),
                ClientId = finalClientId,
                MasterId = dto.MasterId,
                ServiceId = dto.ServiceId,
                Start_date = exactTime, 
                Notes = dto.Notes,

                Status = AppointmentStatus.SCHEDULED
            };

            await _Repository.CreateAsync(appointment);
            return Ok(new { success = true, clientId = finalClientId });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var booking = await _Repository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = "No booking found" });
            }

            if (Enum.TryParse<AppointmentStatus>(dto.NewStatus, true, out var parsedStatus))
            {
                booking.Status = parsedStatus;
                await _Repository.UpdateAsync(id, booking);

                return Ok(new { message = $"Status successfully changed to {parsedStatus}" });
            }

            return BadRequest(new { message = "Invalid status" });
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
                    price = service?.ServicePrice ?? 0,
                    notes = a.Notes
                };
            }).ToList();

            return Ok(userBookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAdminList()
        {
            var appointments = await _Repository.GetAllAsync();
            var services = await _ServiceRepo.GetAllAsync();
            var masters = await _MasterRepo.GetAllAsync();
            var clients = await _ClientRepo.GetAllAsync(); 

            var adminBookings = appointments
                .OrderByDescending(a => a.Start_date)
                .Select(a => {
                    var service = services.FirstOrDefault(s => s.Id == a.ServiceId);
                    var master = masters.FirstOrDefault(m => m.Id == a.MasterId);
                    var client = clients.FirstOrDefault(c => c.Id == a.ClientId);

                    return new
                    {
                        id = a.Id.ToString(),
                        bookingId = "BS-" + a.Id.ToString().Substring(0, 8).ToUpper(),

                        clientName = client?.Name ?? "Unknown",
                        clientPhone = client?.Phone ?? "No number",
                        clientEmail = client?.Email ?? "-",

                        serviceName = service?.Title ?? "Deleted service",
                        masterName = master?.Name ?? "‘Any master",

                        date = a.Start_date.ToString("yyyy-MM-dd"),
                        time = a.Start_date.ToString("HH:mm"),
                        status = a.Status.ToString(),
                        price = service?.ServicePrice ?? 0,
                        notes = a.Notes
                    };
                }).ToList();

            return Ok(adminBookings);
        }

        [Authorize(Roles = "Master,Admin")] 
        [HttpGet("master-list")]
        public async Task<IActionResult> GetMasterBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;

            var clients = await _ClientRepo.GetAllAsync();
            var me = clients.FirstOrDefault(c => c.Id.ToString() == userId);

            if (me == null || me.MasterProfileId == null || me.MasterProfileId == Guid.Empty)
            {
                return BadRequest(new { message = "No master profile is linked to this account. Please contact the administrator." });
            }

            var allAppointments = await _Repository.GetAllAsync();
            var services = await _ServiceRepo.GetAllAsync();

            var myAppointments = allAppointments
                .Where(a => a.MasterId == me.MasterProfileId)
                .OrderByDescending(a => a.Start_date)
                .Select(a => {
                    var service = services.FirstOrDefault(s => s.Id == a.ServiceId);
                    var client = clients.FirstOrDefault(c => c.Id == a.ClientId);

                    return new
                    {
                        id = a.Id.ToString(),
                        bookingId = "BS-" + a.Id.ToString().Substring(0, 8).ToUpper(),
                        clientName = client?.Name ?? "Unknown",
                        clientPhone = client?.Phone ?? "No number",
                        serviceName = service?.Title ?? "Removed service",
                        date = a.Start_date.ToString("yyyy-MM-dd"),
                        time = a.Start_date.ToString("HH:mm"),
                        status = a.Status.ToString(),
                        price = service?.ServicePrice ?? 0,
                        notes = a.Notes
                    };
                }).ToList();

            return Ok(myAppointments);
        }
    }
}

