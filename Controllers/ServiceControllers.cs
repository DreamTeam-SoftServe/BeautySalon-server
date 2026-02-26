using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Domain.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Domain.Enum;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/Service")]
    public class ServiceControllers : Controller
    {
        private readonly IRepository<Service> _Repository;
        private readonly IImageService _imageService;

        public ServiceControllers(IRepository<Service> repository, IImageService imageService)
        {
            _Repository = repository;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _Repository.GetAllAsync();
            var result = services.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                duration = s.Duration,
                description = s.Description,
                servicePrice = s.ServicePrice,
                serviceType = (int)s.ServiceType,
                imageUrl = s.ImageUrl
            });
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceDto dto)
        {
            var service = new Service
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                ServicePrice = dto.ServicePrice,
                Duration = dto.Duration,
                ImageUrl = dto.ImageUrl,
                ServiceType = (Domain.Enum.ServiceType)dto.ServiceType
            };
            await _Repository.CreateAsync(service);
            return CreatedAtAction(nameof(GetAll), new { id = service.Id }, service);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ServiceDto dto)
        {
            var service = await _Repository.GetByIdAsync(id);
            if (service == null)
                return NotFound(new { message = "Service not found" });

            if (!string.IsNullOrEmpty(service.ImageUrl) && service.ImageUrl != dto.ImageUrl)
                await _imageService.DeleteImageAsync(service.ImageUrl);

            service.Title = dto.Title;
            service.Description = dto.Description;
            service.ServicePrice = dto.ServicePrice;
            service.Duration = dto.Duration;
            service.ImageUrl = dto.ImageUrl;
            service.ServiceType = (Domain.Enum.ServiceType)dto.ServiceType;

            await _Repository.UpdateAsync(id, service);
            return Ok(service);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var service = await _Repository.GetByIdAsync(id);
            if (service == null)
                return NotFound(new { message = "Service not found" });

            if (!string.IsNullOrEmpty(service.ImageUrl))
                await _imageService.DeleteImageAsync(service.ImageUrl);

            await _Repository.DeleteAsync(id);
            return NoContent();
        }
    }
}