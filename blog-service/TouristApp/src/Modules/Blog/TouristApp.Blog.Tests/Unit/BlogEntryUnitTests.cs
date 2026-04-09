using Shouldly;
using TouristApp.Blog.Core.Domain;
using System;
using System.Collections.Generic;
using Xunit;

namespace TouristApp.Blog.Tests.UnitTest.Domain;

public class BlogEntryTests
{
    [Fact]
    public void Creates()
    {
        // Arrange & Act
        var blog = new Core.Domain.Blog(
            "Izlet na planinu",
            "**Prelepo** iskustvo sa puno slika.",
            new DateTime(2026, 5, 10),
            new List<string> { "slika1.png" });

        // Assert
        blog.Title.ShouldBe("Izlet na planinu");
        blog.Description.ShouldBe("**Prelepo** iskustvo sa puno slika.");
        blog.Images.Count.ShouldBe(1);
    }

    [Fact]
    public void Fails_with_empty_title()
    {
        Should.Throw<ArgumentException>(() => new Core.Domain.Blog(
            "",
            "Opis",
            DateTime.Now,
            null));
    }
}