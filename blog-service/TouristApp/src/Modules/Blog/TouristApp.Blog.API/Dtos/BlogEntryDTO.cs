namespace TouristApp.Blog.API.Dtos;

public class BlogEntryDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; } // Očekuje se Markdown tekst
    public DateTime CreationDate { get; set; }
    public List<string>? Images { get; set; }
    public List<CommentDto>? Comments { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}