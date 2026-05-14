using AutoMapper;
using Moq;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.Core.Domain;
using TouristApp.Tours.Core.Domain.Repositories;
using TouristApp.Tours.Core.Mappers;
using TouristApp.Tours.Core.UseCases;

namespace TouristApp.Tours.Tests.Unit;

public class TourServiceTests
{
    private readonly Mock<ITourRepository> _repoMock;
    private readonly TourService _service;

    public TourServiceTests()
    {
        _repoMock = new Mock<ITourRepository>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<ToursProfile>())
            .CreateMapper();
        _service = new TourService(_repoMock.Object, mapper);
    }

    [Fact]
    public void Create_sets_draft_and_zero_price_and_saves_tour()
    {
        Tour? savedTour = null;

        _repoMock
            .Setup(r => r.Add(It.IsAny<Tour>()))
            .Callback<Tour>(tour => savedTour = tour);

        var dto = new CreateTourDto(
            Name: "Fruska Gora vikend ruta",
            Description: "Lagana planinarska ruta za vikend.",
            Difficulty: "easy",
            Tags: new List<string> { "planina", "vikend" }
        );

        var result = _service.Create(authorId: 101, dto);

        result.ShouldNotBeNull();
        result.AuthorId.ShouldBe(101);
        result.Name.ShouldBe("Fruska Gora vikend ruta");
        result.Difficulty.ShouldBe("Easy");
        result.Status.ShouldBe("Draft");
        result.Price.ShouldBe(0m);

        savedTour.ShouldNotBeNull();
        savedTour.Status.ShouldBe(TourStatus.Draft);
        savedTour.Price.ShouldBe(0m);
        _repoMock.Verify(r => r.Add(It.IsAny<Tour>()), Times.Once);
    }

    [Fact]
    public void Create_invalid_difficulty_throws_and_does_not_save()
    {
        var dto = new CreateTourDto(
            Name: "Test",
            Description: "Opis",
            Difficulty: "impossible",
            Tags: new List<string>()
        );

        Should.Throw<EntityValidationException>(
            () => _service.Create(authorId: 5, dto));

        _repoMock.Verify(r => r.Add(It.IsAny<Tour>()), Times.Never);
    }

    [Fact]
    public void GetByAuthor_returns_paged_mapped_tours()
    {
        var tours = new List<Tour>
        {
            Tour.Create(7, "Tura A", "Opis A", TourDifficulty.Medium, new List<string> { "grad" }),
            Tour.Create(7, "Tura B", "Opis B", TourDifficulty.Hard, new List<string> { "avantura" })
        };

        _repoMock
            .Setup(r => r.GetByAuthorId(7, 1, 10))
            .Returns(new PagedResult<Tour>(tours, 2));

        var result = _service.GetByAuthor(7, 1, 10);

        result.TotalCount.ShouldBe(2);
        result.Results.Count.ShouldBe(2);
        result.Results.All(t => t.AuthorId == 7).ShouldBeTrue();
        result.Results[0].Status.ShouldBe("Draft");
        result.Results[0].Price.ShouldBe(0m);
        result.Results[1].Difficulty.ShouldBe("Hard");
    }

    [Fact]
    public void Delete_by_author_and_draft_calls_repo_delete()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        _repoMock.Setup(r => r.GetById(5)).Returns(tour);

        _service.Delete(5, 1);

        _repoMock.Verify(r => r.Delete(tour), Times.Once);
    }

    [Fact]
    public void Delete_by_non_author_throws()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        _repoMock.Setup(r => r.GetById(6)).Returns(tour);

        Should.Throw<EntityValidationException>(() => _service.Delete(6, 999));
        _repoMock.Verify(r => r.Delete(It.IsAny<Tour>()), Times.Never);
    }

    [Fact]
    public void Delete_when_not_draft_throws()
    {
        var tour = Tour.Create(1, "Tura", "Opis", TourDifficulty.Easy, new List<string>());
        // mark published
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.Publish();

        _repoMock.Setup(r => r.GetById(7)).Returns(tour);

        Should.Throw<EntityValidationException>(() => _service.Delete(7, 1));
        _repoMock.Verify(r => r.Delete(It.IsAny<Tour>()), Times.Never);
    }

    [Fact]
    public void Update_by_author_and_draft_updates_and_saves()
    {
        var tour = Tour.Create(1, "Old", "Old Desc", TourDifficulty.Easy, new List<string>());
        _repoMock.Setup(r => r.GetById(10)).Returns(tour);

        var dto = new UpdateTourDto("New", "New Desc", "Medium", new List<string>{"a"}, 12.34m);

        _service.Update(10, 1, dto);

        _repoMock.Verify(r => r.Update(It.Is<Tour>(t => t.Name == "New" && t.Description == "New Desc" && t.Price == 12.34m)), Times.Once);
    }

    [Fact]
    public void Update_by_non_author_throws()
    {
        var tour = Tour.Create(1, "Old", "Old Desc", TourDifficulty.Easy, new List<string>());
        _repoMock.Setup(r => r.GetById(11)).Returns(tour);

        var dto = new UpdateTourDto("New", "New", "Easy", new List<string>(), 0m);

        Should.Throw<EntityValidationException>(() => _service.Update(11, 999, dto));
        _repoMock.Verify(r => r.Update(It.IsAny<Tour>()), Times.Never);
    }

    [Fact]
    public void Update_when_not_draft_throws()
    {
        var tour = Tour.Create(1, "Old", "Old Desc", TourDifficulty.Easy, new List<string>());
        tour.AddKeyPoint(new KeyPoint(1, "A", "a", "s", "i.jpg", 44, 20));
        tour.AddKeyPoint(new KeyPoint(2, "B", "b", "s", "j.jpg", 44.1, 20.1));
        tour.Publish();

        _repoMock.Setup(r => r.GetById(12)).Returns(tour);

        var dto = new UpdateTourDto("New", "New", "Easy", new List<string>(), 0m);

        Should.Throw<EntityValidationException>(() => _service.Update(12, 1, dto));
    }
}

