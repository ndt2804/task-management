using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using task_management.Data;
using task_management.Entities;
using task_management.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IMongoCollection<User>? _user;
        private readonly IConfiguration _configuration;
        public AuthController(MongoDbServices mongoDbServices, IConfiguration configuration)
        {
            _user = mongoDbServices.Database?.GetCollection<User>("User");
            _configuration = configuration;
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
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var newUser = new User
            {
                Username = user.Username,
                Email = user.Email,
                Password = passwordHash,
            };

            await _user.InsertOneAsync(newUser);
            return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
        }
        [HttpPost]
        [Route("login")]

        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _user.Find(u => u.Email == loginModel.Email).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }
            var token = GenerateJwtToken(user);
            var result = new
            {
                Message = $"Welcome back, {user.Email}! :)",
                Token = token
            };

            return Ok(result);
        }
        [HttpPost]
        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1), 
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
