using TouristApp.BuildingBlocks.Core.Domain;

namespace TouristApp.Blog.Core.Domain;

public class Comment : Entity
{
    public long AuthorId { get; init; }
    public string Text { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; private set; }
    public long BlogId { get; init; }

    private Comment() { }

    public Comment(long authorId, string text, long blogId)
    {
        if (authorId <= 0) throw new ArgumentException("AuthorId must be valid.");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text is required.");

        AuthorId = authorId;
        Text = text;
        BlogId = blogId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string newText, long requesterId)
    {
        if (requesterId != AuthorId)
            throw new UnauthorizedAccessException("Only the author can edit a comment.");
        if (string.IsNullOrWhiteSpace(newText))
            throw new ArgumentException("Text is required.");

        Text = newText;
        LastModifiedAt = DateTime.UtcNow;
    }
}