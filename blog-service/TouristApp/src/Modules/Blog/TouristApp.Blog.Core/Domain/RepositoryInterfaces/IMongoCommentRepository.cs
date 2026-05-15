using TouristApp.Blog.Core.Domain;

namespace TouristApp.Blog.Core.Domain.RepositoryInterfaces;

public interface IMongoCommentRepository
{
    void Save(Comment comment);
    void Update(Comment comment);
    void Delete(long postgresCommentId);
}