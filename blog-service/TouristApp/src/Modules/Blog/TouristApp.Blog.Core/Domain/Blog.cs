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
}