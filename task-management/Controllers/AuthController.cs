using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using task_management.Data;
using task_management.Entities;

namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IMongoCollection<User>? _user;
        public AuthController(MongoDbServices mongoDbServices)
        {
            _user = mongoDbServices.Database?.GetCollection<User>("User");

        }
        [HttpGet]
        public async Task<IEnumerable<User>> GetUser()
        {
            return await _user.Find(FilterDefinition<User>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User?>> GetById(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var user = _user.Find(filter).FirstOrDefault();
            return user is not null ? Ok(user) : NotFound();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var existingUser = await _user.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new Exception("User already exists.");
            }

            var newUser = new User
            {
                Username = user.Username,
                Email = user.Email,
                Password = HashPassword(user.Password) 
            };

            await _user.InsertOneAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPost]
        public async Task<ActionResult<User?>> Create(User user)
        {
            await _user.InsertOneAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
     

        private string HashPassword(string password)
        {
            using (var hmac = new HMACSHA256())
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            using (var hmac = new HMACSHA256())
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(hashBytes);
            }
        }
    }
}
