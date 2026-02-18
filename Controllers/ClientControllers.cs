using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientControllers : Controller
    {
        private readonly IRepository<Client> _Repository;
        public ClientControllers(IRepository<Client> repository)
        {
            _Repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _Repository.GetAllAsync();
            return Ok(clients);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            await _Repository.CreateAsync(client);
            return CreatedAtAction(nameof(GetAll), new { id = client.Id }, client);
        }
    }
}
