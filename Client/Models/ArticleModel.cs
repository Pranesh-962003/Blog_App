using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Client.Models
{
    public class ArticleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Heading { get; set; } = null!;
        public string ArticleBody { get; set; } = null!;
        public string Email { get; set; } = null!; // Author email
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; } = null!;

        public List<string> Likes { get; set; } = new();
    }
    public class UserModel
    {
        [BsonId] // Marks this property as the document ID
        [BsonRepresentation(BsonType.ObjectId)] // Let MongoDB map string to ObjectId
        public string Id { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("subscribers")]
        public List<string> Subscribers { get; set; } = new List<string>();

        [BsonElement("subscribedTo")]
        public List<string> SubscribedTo { get; set; } = new List<string>();
    }

}
