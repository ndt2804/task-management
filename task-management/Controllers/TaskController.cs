using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using task_management.Data;
using task_management.Entities;

namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IMongoCollection<TaskItem>? _task;
        private readonly IConfiguration _configuration;
        public TaskController(MongoDbServices mongoDbServices, IConfiguration configuration)
        {
            _task = mongoDbServices.Database?.GetCollection<TaskItem>("Task");
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _task.Find(_ => true).ToListAsync();
            return Ok(tasks);
        }
        [HttpGet("getTask/{id}")]
        public async Task<IActionResult> GetTaskById(string id)
        {
            var task = await _task.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null)
            {
                return NotFound("Task not found");
            }
            return Ok(task);
        }
        [HttpPost("createTask")]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem taskItem)
        {
            if (taskItem == null)
            {
                return BadRequest("Task cannot be null");
            }
            taskItem.CreatedAt = DateTime.Now;
            taskItem.UpdatedAt = DateTime.Now;
            await _task.InsertOneAsync(taskItem);

            return CreatedAtAction(nameof(GetTasks), new { id = taskItem.Id }, taskItem);
        }
        [HttpPut("updateTask/{id}")]
        public async Task<IActionResult> UpdateTask(string id, [FromBody] TaskItem updatedTask)
        {
            if (updatedTask == null)
            {
                return BadRequest("Task cannot be null");
            }
            var existingTask = await _task.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTask == null)
            {
                return NotFound("Task not found");
            }

            updatedTask.Id = existingTask.Id; 
            updatedTask.CreatedAt = existingTask.CreatedAt; 
            updatedTask.UpdatedAt = DateTime.Now; 

            await _task.ReplaceOneAsync(t => t.Id == id, updatedTask);

            return Ok("Update successfully"); // Trả về mã 204 khi cập nhật thành công
        }
        [HttpDelete("deleteTask/{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            var task = await _task.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null)
            {
                return NotFound("Task not found");
            }

            await _task.DeleteOneAsync(t => t.Id == id);

            return Ok("Delete successfully"); // Trả về mã 204 khi cập nhật thành công
        }

        [HttpPut("markcomplete/{id}")]
        public async Task<IActionResult> MarkComplete(string id)
        {
            var task = await _task.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null)
            {
                return NotFound();
            }

            task.IsCompleted = true;
            task.UpdatedAt = DateTime.Now;

            // Cập nhật dữ liệu trong MongoDB
            await _task.ReplaceOneAsync(t => t.Id == task.Id, task);

            return Ok("Successfully Complete Task");
        }

        [HttpPut("markincomplete/{id}")]
        public async Task<IActionResult> MarkIncomplete(string id)
        {
            var task = await _task.Find(t => t.Id == id ).FirstOrDefaultAsync();
            if (task == null)
            {
                return NotFound();
            }

            task.IsCompleted = false;
            task.UpdatedAt = DateTime.Now;

            // Cập nhật lại tác vụ
            await _task.ReplaceOneAsync(t => t.Id == task.Id, task);

            return Ok("UnSuccessfully Complete Task");
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetTasksByCompletionStatus([FromQuery] bool? completed)
        {
            var filter = Builders<TaskItem>.Filter.Empty;
            if (completed.HasValue)
            {
                filter = Builders<TaskItem>.Filter.Eq(t => t.IsCompleted, completed.Value);
            }

            var tasks = await _task.Find(filter).ToListAsync();
            return Ok(tasks);
        }
    }
}
