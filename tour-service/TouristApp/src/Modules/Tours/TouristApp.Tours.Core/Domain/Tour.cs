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
    public IReadOnlyList<TourTravelTime> TravelTimes => _travelTimes.AsReadOnly();
    public TourStatus Status { get; private set; }
    public decimal Price { get; private set; }
    public decimal RouteLengthKm { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public IReadOnlyList<KeyPoint> KeyPoints => _keyPoints.AsReadOnly();

    private List<string> _tags = new();
    private List<TourTravelTime> _travelTimes = new();
    private List<KeyPoint> _keyPoints = new();

    // Required by EF Core
    private Tour() { }

    private Tour(
        long authorId,
        string name,
        string description,
        TourDifficulty difficulty,
        List<string> tags,
        List<TourTravelTime> travelTimes)
    {
        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        _tags = tags;
        _travelTimes = travelTimes;

        //Status se uvek postavlja na Draft, cena na 0
        Status = TourStatus.Draft;
        Price = 0m;
        RouteLengthKm = 0m;
        PublishedAt = null;
        ArchivedAt = null;
    }

    /// <summary>
    /// Status se automatski postavlja na Draft, cena na 0.
    /// </summary>
    public static Tour Create(
        long authorId,
        string name,
        string description,
        TourDifficulty difficulty,
        List<string> tags,
        List<TourTravelTime> travelTimes)
    {
        Validate(authorId, name, description, travelTimes);

        return new Tour(authorId, name, description, difficulty, tags ?? new List<string>(), travelTimes);
    }

    private static void Validate(long authorId, string name, string description, List<TourTravelTime> travelTimes)
    {
        if (authorId <= 0)
            throw new EntityValidationException("ID autora mora biti pozitivan broj.");

        ValidateDetails(name, description);
        ValidateTravelTimes(travelTimes);
    }

    private static void ValidateDetails(string name, string description)
    {
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
        EnsureEditable();

        if (keyPoint == null)
            throw new ArgumentNullException(nameof(keyPoint), "Ključna tačka ne sme biti null.");

        // Allow inserting at a specific position. If ordinal is greater than count+1, it's invalid.
        var desiredOrdinal = keyPoint.OrdinalNo;
        var maxOrdinal = _keyPoints.Count + 1;
        if (desiredOrdinal > maxOrdinal)
            throw new EntityValidationException($"Redni broj ključne tačke ne sme biti veći od {maxOrdinal}.");

        // Shift existing keypoints with ordinal >= desiredOrdinal up by 1
        foreach (var kp in _keyPoints.Where(k => k.OrdinalNo >= desiredOrdinal))
        {
            kp.UpdateOrdinalNo(kp.OrdinalNo + 1);
        }

        _keyPoints.Add(keyPoint);
        RecalculateKeyPointOrdinals();
        RecalculateRouteLengthKm();
    }

    /// <summary>
    /// Uklanja ključnu tačku iz ture prema rednom broju.
    /// </summary>
    public void RemoveKeyPoint(int ordinalNo)
    {
        EnsureEditable();

        var kp = _keyPoints.FirstOrDefault(k => k.OrdinalNo == ordinalNo);
        if (kp != null)
        {
            _keyPoints.Remove(kp);
            RecalculateKeyPointOrdinals();
            RecalculateRouteLengthKm();
        }
    }

    /// <summary>
    /// Ažurira ključnu tačku prema rednom broju.
    /// </summary>
    public void UpdateKeyPoint(int ordinalNo, KeyPointUpdate update)
    {
        EnsureEditable();

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
        RecalculateRouteLengthKm();
    }

    /// <summary>
    /// Briše sve ključne tačke iz ture.
    /// </summary>
    public void ClearKeyPoints()
    {
        _keyPoints.Clear();
        RecalculateRouteLengthKm();
    }

    public void UpdateDetails(string name, string description, TourDifficulty difficulty, List<string> tags, decimal price)
    {
        ValidateDetails(name, description);

        Name = name;
        Description = description;
        Difficulty = difficulty;
        _tags = tags ?? new List<string>();
        Price = price;
    }

    public void SetTravelTimes(List<TourTravelTime> travelTimes)
    {
        ValidateTravelTimes(travelTimes);
        _travelTimes = travelTimes;
    }

    public void SetRouteLengthKm(decimal routeLengthKm)
    {
        if (routeLengthKm < 0)
            throw new EntityValidationException("Dužina ture ne može biti negativna.");

        RouteLengthKm = decimal.Round(routeLengthKm, 2, MidpointRounding.AwayFromZero);
    }

    private void EnsureEditable()
    {
        if (Status != TourStatus.Draft)
            throw new EntityValidationException("Turu je moguće menjati samo dok je u stanju Draft.");
    }

    /// <summary>
    /// Publikuje turu (Draft -> Published) samo ako sadrži najmanje dve ključne tačke.
    /// </summary>
    public void Publish()
    {
        ValidateDetails(Name, Description);

        if (_tags == null || _tags.Count == 0 || _tags.Any(string.IsNullOrWhiteSpace))
            throw new EntityValidationException("Tura mora sadržati bar jedan ispravan tag da bi bila publikovana.");

        ValidateTravelTimes(_travelTimes);

        if (_keyPoints.Count < 2)
            throw new EntityValidationException("Tura mora imati najmanje dve ključne tačke da bi bila publikovana.");

        PublishedAt = DateTime.UtcNow;
        ArchivedAt = null;
        Status = TourStatus.Published;
    }

    /// <summary>
    /// Arhivira turu (Published -> Archived).
    /// </summary>
    public void Archive()
    {
        if (Status != TourStatus.Published)
            throw new EntityValidationException("Samo objavljenu turu je moguće arhivirati.");

        ArchivedAt = DateTime.UtcNow;
        Status = TourStatus.Archived;
    }

    /// <summary>
    /// Ponovo aktivira arhiviranu turu (Archived -> Published).
    /// </summary>
    public void Reactivate()
    {
        if (Status != TourStatus.Archived)
            throw new EntityValidationException("Samo arhiviranu turu je moguće ponovo aktivirati.");

        PublishedAt = DateTime.UtcNow;
        ArchivedAt = null;
        Status = TourStatus.Published;
    }

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

    private void RecalculateRouteLengthKm()
    {
        if (_keyPoints.Count < 2)
        {
            RouteLengthKm = 0m;
            return;
        }

        var ordered = _keyPoints.OrderBy(point => point.OrdinalNo).ToList();
        double totalKm = 0;

        for (int i = 1; i < ordered.Count; i++)
        {
            totalKm += HaversineDistanceKm(
                ordered[i - 1].Latitude,
                ordered[i - 1].Longitude,
                ordered[i].Latitude,
                ordered[i].Longitude);
        }

        RouteLengthKm = decimal.Round((decimal)totalKm, 2, MidpointRounding.AwayFromZero);
    }

    private static double HaversineDistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double EarthRadiusKm = 6371d;

        var lat1 = DegreesToRadians(latitude1);
        var lon1 = DegreesToRadians(longitude1);
        var lat2 = DegreesToRadians(latitude2);
        var lon2 = DegreesToRadians(longitude2);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var a = Math.Pow(Math.Sin(deltaLat / 2), 2)
            + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private static void ValidateTravelTimes(List<TourTravelTime> travelTimes)
    {
        if (travelTimes == null || travelTimes.Count == 0)
            throw new EntityValidationException("Morate definisati bar jedno vreme obilaska za neki tip prevoza.");

        if (travelTimes.Any(time => !Enum.IsDefined(typeof(TransportType), time.TransportType)))
            throw new EntityValidationException("Tip prevoza mora biti jedna od vrednosti: Walking, Bicycle, Car.");

        if (travelTimes.Any(time => time.Minutes <= 0))
            throw new EntityValidationException("Vreme obilaska mora biti pozitivan broj minuta.");

        if (travelTimes.Select(time => time.TransportType).Distinct().Count() != travelTimes.Count)
            throw new EntityValidationException("Za svaki tip prevoza može postojati samo jedno vreme obilaska.");
    }
}