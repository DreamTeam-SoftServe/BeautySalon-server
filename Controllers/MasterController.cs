using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Domain.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : Controller
    {
        private readonly IRepository<Master> _Repository;

        public MasterController(IRepository<Master> repository)
        {
            _Repository = repository;
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
    }
}
