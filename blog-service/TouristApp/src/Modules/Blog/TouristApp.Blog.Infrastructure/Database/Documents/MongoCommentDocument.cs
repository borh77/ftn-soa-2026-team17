using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace TouristApp.Blog.Infrastructure.Database.Documents;

public class MongoCommentDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public long PostgresCommentId { get; set; }
    public long BlogId { get; set; }
    public long AuthorId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}