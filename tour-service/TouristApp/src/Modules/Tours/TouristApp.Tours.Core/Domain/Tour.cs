using TouristApp.BuildingBlocks.Core.Domain;
using TouristApp.BuildingBlocks.Core.Exceptions;
using TouristApp.Tours.Core.Domain;

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
    public IReadOnlyList<KeyPoint> KeyPoints => _keyPoints.AsReadOnly();

    private List<string> _tags = new();
    private List<KeyPoint> _keyPoints = new();

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

    /// <summary>
    /// Dodaje ključnu tačku u turu.
    /// </summary>
    public void AddKeyPoint(KeyPoint keyPoint)
    {
        if (keyPoint == null)
            throw new ArgumentNullException(nameof(keyPoint), "Ključna tačka ne sme biti null.");

        if (_keyPoints.Any(k => k.OrdinalNo == keyPoint.OrdinalNo))
            throw new EntityValidationException($"Ključna tačka sa rednim brojem {keyPoint.OrdinalNo} već postoji u turi.");

        _keyPoints.Add(keyPoint);
        RecalculateKeyPointOrdinals();
    }

    /// <summary>
    /// Uklanja ključnu tačku iz ture prema rednom broju.
    /// </summary>
    public void RemoveKeyPoint(int ordinalNo)
    {
        var kp = _keyPoints.FirstOrDefault(k => k.OrdinalNo == ordinalNo);
        if (kp != null)
        {
            _keyPoints.Remove(kp);
            RecalculateKeyPointOrdinals();
        }
    }

    /// <summary>
    /// Ažurira ključnu tačku prema rednom broju.
    /// </summary>
    public void UpdateKeyPoint(int ordinalNo, KeyPointUpdate update)
    {
        if (update == null)
            throw new ArgumentNullException(nameof(update), "Ažuriranje ključne tačke ne sme biti null.");

        var keyPoint = _keyPoints.FirstOrDefault(k => k.OrdinalNo == ordinalNo);
        if (keyPoint == null)
            throw new EntityValidationException($"Ključna tačka sa rednim brojem {ordinalNo} nije pronađena.");

        var updatedKeyPoint = new KeyPoint(
            ordinalNo,
            update.Name ?? keyPoint.Name,
            update.Description ?? keyPoint.Description,
            update.SecretText ?? keyPoint.SecretText,
            update.ImageUrl ?? keyPoint.ImageUrl,
            update.Latitude,
            update.Longitude
        );

        _keyPoints[_keyPoints.IndexOf(keyPoint)] = updatedKeyPoint;
    }

    /// <summary>
    /// Briše sve ključne tačke iz ture.
    /// </summary>
    public void ClearKeyPoints() => _keyPoints.Clear();

    /// <summary>
    /// Rekalkulator rednih brojeva ključnih tačaka.
    /// </summary>
    private void RecalculateKeyPointOrdinals()
    {
        var ordered = _keyPoints.OrderBy(k => k.OrdinalNo).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].UpdateOrdinalNo(i + 1);
        }
        _keyPoints.Clear();
        _keyPoints.AddRange(ordered);
    }
}