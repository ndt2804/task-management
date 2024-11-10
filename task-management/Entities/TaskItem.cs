using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace task_management.Entities
{
    public class TaskItem
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public String? Id { get; set; }

        [BsonElement("nametask")]
        [Required(ErrorMessage = "Task name is required")]
        [StringLength(100, ErrorMessage = "Task name can't be longer than 100 characters.")]
        public String Name { get; set; }

        [BsonElement("description")]
        public String Description { get; set; }

        [BsonElement("iscompleted")]
        public bool IsCompleted { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }
        [BsonIgnore]  // Ignore this property for database serialization
        public User? User { get; set; }  //
    }
}
