using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthControllers : Controller
    {
        private readonly IRepository<Client> _Repository;
        private readonly IConfiguration _Configuration;
        public AuthControllers(IRepository<Client> repository, IConfiguration configuration)
        {
            _Repository = repository;
            _Configuration = configuration;
        }

        private string GenerateJwtToken(Client client)
        {
            var jwtSettings = _Configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: new[] {
            new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
            new Claim("id", client.Id.ToString())
                },
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email required!" });
            }

            var allClients = await _Repository.GetAllAsync();
            bool emailExists = allClients.Any(c =>string.Equals(c?.Email, request?.Email, StringComparison.OrdinalIgnoreCase));
           
            if (emailExists)
            {
                return BadRequest(new { message = "Email is already in use." });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newClient = new Client
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = passwordHash,
            };

            await _Repository.CreateAsync(newClient);

            return Ok(new { message = "Registration was successful!" });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var allClients = await _Repository.GetAllAsync();
            var client = allClients.FirstOrDefault(c =>
                string.Equals(c?.Email, request?.Email, StringComparison.OrdinalIgnoreCase));

            if (client == null || !BCrypt.Net.BCrypt.Verify(request.Password, client.PasswordHash))
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            var token = GenerateJwtToken(client);

            return Ok(new
            {
                accessToken = token,
                expiresIn = 3600 * 24 * 7,
                user = new
                {
                    id = client.Id,
                    name = client.Name,
                    email = client.Email,
                    phone = client.Phone, 
                    createdAt = client.RegisteredAt 
                }
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var all = await _Repository.GetAllAsync();
            var myClient = all.FirstOrDefault(c => c.Id.ToString() == userId);

            if (myClient == null) return NotFound();

            return Ok(new
            {
                id = myClient.Id,
                name = myClient.Name,
                email = myClient.Email,
                phone = myClient.Phone,
                createdAt = myClient.RegisteredAt
            });
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var all = await _Repository.GetAllAsync();
            var myClient = all.FirstOrDefault(c => c.Id.ToString() == userId);
            var originalDate = myClient.RegisteredAt;

            if (myClient == null) return NotFound();

            if (myClient.Email != dto.Email)
            {
                bool emailExists = all.Any(c => c.Email == dto.Email && c.Id != myClient.Id);
                if (emailExists) return BadRequest(new { message = "Email is already taken" });

                myClient.Email = dto.Email;
            }

            myClient.Name = dto.Name;
            myClient.Phone = dto.Phone;
            myClient.Email = dto.Email;

            myClient.RegisteredAt = originalDate;

            await _Repository.UpdateAsync(myClient.Id, myClient);

            var newToken = GenerateJwtToken(myClient);

            return Ok(new
            {
                accessToken = newToken, 
                user = new
                {
                    id = myClient.Id,
                    name = myClient.Name,
                    email = myClient.Email,
                    phone = myClient.Phone,
                    createdAt = myClient.RegisteredAt 
                }
            });
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            var client = (await _Repository.GetAllAsync())
                         .FirstOrDefault(c => c.Email == dto.Email);

            if (client == null) return NotFound("There is no client with this number.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            client.PasswordHash = passwordHash;
            await _Repository.UpdateAsync(client.Id, client);

            return Ok(new { message = "Password successfully set" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }   
    }
}
