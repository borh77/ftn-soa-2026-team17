WITH inserted_tour AS (
    INSERT INTO "Tours"."Tours" (
        "AuthorId",
        "Name",
        "Description",
        "Difficulty",
        "Status",
        "Price",
        "Tags",
        "PublishedAt",
        "ArchivedAt",
        "RouteLengthKm",
        "TravelTimes"
    )
    VALUES (
        8,
        'RPC test ruta - Dunavski park',
        'Nova objavljena ruta za testiranje RPC komunikacije i TourExecution scenarija.',
        'Easy',
        'Published',
        0,
        '["rpc", "test", "dunavski park"]'::jsonb,
        now(),
        NULL,
        0.80,
        '[{"TransportType":"Walking","Minutes":20}]'::jsonb
    )
    RETURNING "Id"
)
INSERT INTO "Tours"."KeyPoints" (
    "TourId",
    "OrdinalNo",
    "Name",
    "Description",
    "SecretText",
    "ImageUrl",
    "Latitude",
    "Longitude"
)
SELECT
    inserted_tour."Id",
    point."OrdinalNo",
    point."Name",
    point."Description",
    point."SecretText",
    point."ImageUrl",
    point."Latitude",
    point."Longitude"
FROM inserted_tour
CROSS JOIN (
    VALUES
        (1, 'Dunavski park - ulaz', 'Prva tacka nove RPC test rute.', 'RPC tajna 1', 'https://placehold.co/800x500?text=RPC+Start', 45.256610::double precision, 19.848330::double precision),
        (2, 'Muzej Vojvodine', 'Druga tacka nove RPC test rute.', 'RPC tajna 2', 'https://placehold.co/800x500?text=RPC+Finish', 45.258420::double precision, 19.850480::double precision)
) AS point("OrdinalNo", "Name", "Description", "SecretText", "ImageUrl", "Latitude", "Longitude")
RETURNING "TourId";
