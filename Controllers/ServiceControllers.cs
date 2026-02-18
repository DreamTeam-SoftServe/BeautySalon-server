using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Domain.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceControllers : Controller
    {
        private readonly IRepository<Service> _Repository;
        public ServiceControllers(IRepository<Service> repository)
        {
            _Repository = repository;
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
                price = s.ServicePrice, 
                category = s.ServiceType.ToString() 
            });

            return Ok(result); 
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service service)
        {
            await _Repository.CreateAsync(service);
            return CreatedAtAction(nameof(GetAll), new { id = service.Id }, service);

        }
    }
}
