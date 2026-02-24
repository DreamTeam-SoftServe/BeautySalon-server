using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Route("api/Auth")]
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
                    new Claim("id", client.Id.ToString()),
                    new Claim(ClaimTypes.Role, client.Role ?? "Client")
                },
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var allClients = await _Repository.GetAllAsync();
            var existingClient = allClients.FirstOrDefault(c =>
                string.Equals(c.Email, request.Email, StringComparison.OrdinalIgnoreCase));

            if (existingClient != null)
            {
                if (string.IsNullOrEmpty(existingClient.PasswordHash))
                {
                    existingClient.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    existingClient.Name = request.Name ?? existingClient.Name;
                    existingClient.Phone = request.Phone ?? existingClient.Phone;

                    await _Repository.UpdateAsync(existingClient.Id, existingClient);

                    var token = GenerateJwtToken(existingClient);
                    return Ok(new
                    {
                        accessToken = token,
                        user = new { 
                            id = existingClient.Id,
                            name = existingClient.Name,
                            email = existingClient.Email,
                            phone = existingClient.Phone,
                            role = "Client"
                        },
                        message = "Guest account successfully converted to permanent!"
                    });
                }
                else
                {
                    return BadRequest(new { message = "A user with this email address is already registered.." });
                }
            }

            var newClient = new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _Repository.CreateAsync(newClient);

            var newToken = GenerateJwtToken(newClient);
            return Ok(new
            {
                accessToken = newToken,
                user = new
                {
                    id = newClient.Id,
                    name = newClient.Name,
                    email = newClient.Email,
                    phone = newClient.Phone
                },
                message = "Registration successful!"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var allClients = await _Repository.GetAllAsync();
            var client = allClients.FirstOrDefault(c =>
                string.Equals(c?.Email, request?.Email, StringComparison.OrdinalIgnoreCase));

            if (client == null || string.IsNullOrEmpty(client.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, client.PasswordHash))
            {
                return BadRequest(new { message = "Incorrect email or password." });
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
                    createdAt = client.RegisteredAt,
                    role = client.Role ?? "Client"
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
                createdAt = myClient.RegisteredAt,
                role = myClient.Role ?? "Client"
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

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _Repository.GetAllAsync();
            var userList = users.Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email,
                phone = u.Phone,
                registeredAt = u.RegisteredAt,
                role = u.Role ?? "Client",
                masterProfileId = u.MasterProfileId
            }).OrderByDescending(u => u.registeredAt);

            return Ok(userList);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            var allUsers = await _Repository.GetAllAsync();
            var user = allUsers.FirstOrDefault(c => c.Id == id);

            if (user == null) return NotFound(new { message = "User not found" });

            var allowedRoles = new[] { "Admin", "Client", "Master" };
            if (!allowedRoles.Contains(dto.Role)) return BadRequest(new { message = "Invalid role" });

            user.Role = dto.Role;

            if (dto.Role == "Master")
            {
                user.MasterProfileId = dto.MasterProfileId; 
            }
            else
            {
                user.MasterProfileId = null;
            }

            await _Repository.UpdateAsync(id, user);

            return Ok(new { message = "Role updated successfully", newRole = user.Role });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userGuid = Guid.Parse(userIdClaim);

            var client = await _Repository.GetByIdAsync(userGuid);
            if (client == null) return NotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, client.PasswordHash))
            {
                return BadRequest(new { message = "The current password is incorrect." });
            }

            client.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _Repository.UpdateAsync(userGuid, client);

            return Ok(new { message = "Password successfully changed" });
        }

    }
}
