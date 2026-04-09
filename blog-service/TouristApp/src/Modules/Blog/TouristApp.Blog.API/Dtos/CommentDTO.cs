namespace TouristApp.Blog.API.Dtos;

public class CommentDto
{
    public long Id { get; set; }
    public long AuthorId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

public class CreateCommentDto
{
    public string Text { get; set; }
}

public class UpdateCommentDto
{
    public string Text { get; set; }
}