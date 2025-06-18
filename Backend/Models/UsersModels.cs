using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<string> Subscribers { get; set; } = new List<string>();
        public List<string> SubscribedTo { get; set; } = new List<string>();

        public List<string> FavoriteArticles { get; set; } = new();
    }

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

        [BsonElement("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>(); // New field
    }


}
