using TouristApp.BuildingBlocks.Core.Domain;

namespace TouristApp.Blog.Core.Domain;

public class BlogLike : Entity
{
    public long BlogId { get; init; }
    public long UserId { get; init; }
    public DateTime LikedAt { get; init; }

    private BlogLike() { }

    public BlogLike(long blogId, long userId)
    {
        BlogId = blogId;
        UserId = userId;
        LikedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }
}