using TouristApp.BuildingBlocks.Core.Domain;
using TouristApp.BuildingBlocks.Core.Exceptions;

namespace TouristApp.Tours.Core.Domain;

public class Tour : AggregateRoot
{
    public long AuthorId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TourDifficulty Difficulty { get; private set; }
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public TourStatus Status { get; private set; }
    public decimal Price { get; private set; }

    private List<string> _tags = new();

    // Required by EF Core
    private Tour() { }

    private Tour(
        long authorId,
        string name,
        string description,
        TourDifficulty difficulty,
        List<string> tags)
    {
        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        _tags = tags;

        //Status se uvek postavlja na Draft, cena na 0
        Status = TourStatus.Draft;
        Price = 0m;
    }

    /// <summary>
    /// Status se automatski postavlja na Draft, cena na 0.
    /// </summary>
    public static Tour Create(
        long authorId,
        string name,
        string description,
        TourDifficulty difficulty,
        List<string> tags)
    {
        Validate(authorId, name, description);

        return new Tour(authorId, name, description, difficulty, tags ?? new List<string>());
    }

    private static void Validate(long authorId, string name, string description)
    {
        if (authorId <= 0)
            throw new EntityValidationException("ID autora mora biti pozitivan broj.");

        if (string.IsNullOrWhiteSpace(name))
            throw new EntityValidationException("Naziv ture je obavezan.");

        if (name.Length > 200)
            throw new EntityValidationException("Naziv ture ne sme biti duži od 200 karaktera.");

        if (string.IsNullOrWhiteSpace(description))
            throw new EntityValidationException("Opis ture je obavezan.");

        if (description.Length > 5000)
            throw new EntityValidationException("Opis ture ne sme biti duži od 5000 karaktera.");
    }
}