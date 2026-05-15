using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TouristApp.API.Controllers;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using TouristApp.BuildingBlocks.Core.Exceptions;

namespace TouristApp.Tours.Tests.Integration;

[Collection("Sequential")]
public class TourCommandTests : BaseToursIntegrationTest
{
    public TourCommandTests(ToursTestFactory factory) : base(factory) { }

    [Fact]
    public void Create_creates_tour_with_draft_status_and_zero_price()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks;
        var controller = CreateController(scope, userId: authorId.ToString());

        var dto = new CreateTourDto(
            Name: "Novi Sad city walk",
            Description: "Lagana gradska setnja kroz centar.",
            Difficulty: "Medium",
            Tags: new List<string> { "grad", "pesacenje" }
        );

        var result = (CreatedAtActionResult)controller.Create(dto).Result!;

        result.StatusCode.ShouldBe(201);
        var created = result.Value.ShouldBeOfType<TourResponseDto>();
        created.AuthorId.ShouldBe(authorId);
        created.Name.ShouldBe("Novi Sad city walk");
        created.Status.ShouldBe("Draft");
        created.Price.ShouldBe(0m);
    }

    [Fact]
    public void Create_invalid_difficulty_throws_validation_error()
    {
        using var scope = Factory.Services.CreateScope();
        var authorId = DateTime.UtcNow.Ticks + 5000;
        var controller = CreateController(scope, userId: authorId.ToString());

        Should.Throw<EntityValidationException>(
            () => controller.Create(
                new CreateTourDto("Nevalidna", "Opis", "UltraHard", new List<string>())));
    }
}
