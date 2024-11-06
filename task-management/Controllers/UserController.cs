using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using task_management.Data;
using task_management.Entities;

namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User>? _user;
        public UserController(MongoDbServices mongoDbServices)
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
        
        [HttpPost]
        public async Task<ActionResult<User?>> Create(User user)
        {
            await _user.InsertOneAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        [HttpPut]
        public async Task<ActionResult<User?>> Update(User user)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);
            //var update = Builders<User>.Update
            //    .Set(x => x.Name, user.Name)
            //.Set(x => x.Email, user.Email);
            //await _user.UpdateOneAsync(filter, update);

            await _user.ReplaceOneAsync(filter, user);
            return Ok();
        }


    }
}
