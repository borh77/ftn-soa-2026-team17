using TouristApp.BuildingBlocks.Core.Domain;
using System;
using System.Collections.Generic;

namespace TouristApp.Blog.Core.Domain;

public class Blog : Entity
{
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTime CreationDate { get; init; }
    public List<string> Images { get; init; }

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    // Parameterless constructor for EF Core
    private Blog() { }

    public Blog(string title, string description, DateTime creationDate, List<string>? images)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.");

        Title = title;
        Description = description;
        CreationDate = creationDate;
        Images = images ?? new List<string>();
    }

    public Comment AddComment(long authorId, string text)
    {
        var comment = new Comment(authorId, text, Id);
        _comments.Add(comment);
        return comment;
    }

    public void UpdateComment(long commentId, long requesterId, string newText)
    {
        var comment = FindComment(commentId);
        comment.Update(newText, requesterId);
    }

    public void DeleteComment(long commentId, long requesterId)
    {
        var comment = FindComment(commentId);
        if (comment.AuthorId != requesterId)
            throw new UnauthorizedAccessException("Only the author can delete a comment.");
        _comments.Remove(comment);
    }

    private Comment FindComment(long commentId) =>
        _comments.FirstOrDefault(c => c.Id == commentId)
        ?? throw new KeyNotFoundException($"Comment {commentId} not found.");


    private readonly List<BlogLike> _likes = new();
    public IReadOnlyCollection<BlogLike> Likes => _likes.AsReadOnly();

    public BlogLike AddLike(long userId)
    {
        if (_likes.Any(l => l.UserId == userId))
            throw new InvalidOperationException("User already liked this blog.");
        var like = new BlogLike(Id, userId);
        _likes.Add(like);
        return like;
    }

    public void RemoveLike(long userId)
    {
        var like = _likes.FirstOrDefault(l => l.UserId == userId)
            ?? throw new InvalidOperationException("Like not found.");
        _likes.Remove(like);
    }

    public int LikeCount => _likes.Count;
    public bool IsLikedByUser(long userId) => _likes.Any(l => l.UserId == userId);
}