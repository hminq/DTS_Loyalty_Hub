using Infrastructure.Models.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly LoyaltyHubDbContext _db;

        public TestController(LoyaltyHubDbContext db)
        {
            _db = db;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users.ToListAsync();

            // Do not return password hashes in responses
            var result = users.Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.FullName,
                u.PhoneNumber,
                u.UserType,
                u.Status,
                u.CreatedAt,
                u.UpdatedAt
            });

            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> Count()
        {
            var count = await _db.Users.CountAsync();

            return Ok(new
            {
                count
            });
        }

        public class CreateUserRequest
        {
            public string Username { get; set; } = null!;

            public string Email { get; set; } = null!;

            public string Password { get; set; } = null!;
            public string? FullName { get; set; }
            public string? Phone { get; set; }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { error = "username and password are required" });

            // simple SHA256 hash for testing only (not for production)
            string HashPassword(string password)
            {
                using var sha = SHA256.Create();
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(bytes);
            }

            // basic uniqueness checks
            if (await _db.Users.AnyAsync(u => u.Username == req.Username))
                return Conflict(new { error = "username already exists" });

            if (!string.IsNullOrWhiteSpace(req.Phone) && await _db.Users.AnyAsync(u => u.PhoneNumber == req.Phone))
                return Conflict(new { error = "phone already exists" });

            // create user entity
            var user = new Infrastructure.Models.User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = HashPassword(req.Password),
                FullName = req.FullName,
                PhoneNumber = req.Phone,
                UserType = "CUSTOMER",
                Status = "ENABLE",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.UserId }, new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.UserType,
                user.Status,
                user.CreatedAt,
                user.UpdatedAt
            });
        }
    }
}
