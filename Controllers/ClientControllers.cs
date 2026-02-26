using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/Client")]
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _Repository.GetByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                await _Repository.DeleteAsync(id);

                return Ok(new { message = "User successfully deleted" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<IActionResult> GetClientsCount()
        {
            var clients = await _Repository.GetAllAsync();
            var count = clients.Count();

            return Ok(count);
        }
    }
}
