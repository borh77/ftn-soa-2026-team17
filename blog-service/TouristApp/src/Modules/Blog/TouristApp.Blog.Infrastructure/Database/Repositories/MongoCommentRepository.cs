using MongoDB.Driver;
using TouristApp.Blog.Core.Domain;
using TouristApp.Blog.Infrastructure.Database.Documents;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;

namespace TouristApp.Blog.Infrastructure.Database.Repositories;

public class MongoCommentRepository : IMongoCommentRepository
{
    private readonly IMongoCollection<MongoCommentDocument> _comments;

    public MongoCommentRepository()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
            ?? "mongodb://localhost:27017";

        var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE")
            ?? "blog";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _comments = database.GetCollection<MongoCommentDocument>("comments");
    }

    public void Save(Comment comment)
    {
        var document = new MongoCommentDocument
        {
            PostgresCommentId = comment.Id,
            BlogId = comment.BlogId,
            AuthorId = comment.AuthorId,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            LastModifiedAt = comment.LastModifiedAt
        };

        _comments.InsertOne(document);
    }

    public void Update(Comment comment)
    {
        var filter = Builders<MongoCommentDocument>.Filter.Eq(
            c => c.PostgresCommentId,
            comment.Id);

        var update = Builders<MongoCommentDocument>.Update
            .Set(c => c.Text, comment.Text)
            .Set(c => c.LastModifiedAt, comment.LastModifiedAt);

        _comments.UpdateOne(filter, update);
    }

    public void Delete(long postgresCommentId)
    {
        var filter = Builders<MongoCommentDocument>.Filter.Eq(
            c => c.PostgresCommentId,
            postgresCommentId);

        _comments.DeleteOne(filter);
    }
}