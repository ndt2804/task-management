using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
namespace task_management.Entities
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
  
        public String? Id { get; set; }
        [BsonElement("username"), BsonRepresentation(BsonType.String)]
        [Required]
        public String? Username { get; set; }

        [BsonElement("email"), BsonRepresentation(BsonType.String)]
        public String? Email { get; set; }

        [BsonElement("password"), BsonRepresentation(BsonType.String)]
        public String? Password { get; set; }

    }
}
