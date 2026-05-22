INSERT INTO "Tours"."Tours" ("AuthorId", "Name", "Description", "Difficulty", "Tags", "Status", "Price", "TravelTimes")
VALUES
(101, 'Fruska Gora test ruta', 'Seed tura za query testove.', 'Easy', '["planina", "vikend"]'::jsonb, 'Draft', 0, '[{{"TransportType":0,"Minutes":120}}]'::jsonb),
(101, 'Dunavska staza', 'Jos jedna seed tura autora 101.', 'Medium', '["reka"]'::jsonb, 'Published', 0, '[{{"TransportType":0,"Minutes":120}},{{"TransportType":1,"Minutes":45}}]'::jsonb),
(202, 'Tarin izazov', 'Seed tura drugog autora.', 'Hard', '["avantura"]'::jsonb, 'Draft', 0, '[{{"TransportType":2,"Minutes":30}}]'::jsonb);

-- Insert keypoints into separate table referencing created tours
INSERT INTO "Tours"."KeyPoints" ("TourId", "OrdinalNo", "Name", "Description", "SecretText", "ImageUrl", "Latitude", "Longitude")
VALUES
((SELECT "Id" FROM "Tours"."Tours" WHERE "Name" = 'Fruska Gora test ruta'), 1, 'Start', 'Pocetna tacka', 'Secret 1', 'start.jpg', 45.123, 19.876),
((SELECT "Id" FROM "Tours"."Tours" WHERE "Name" = 'Dunavska staza'), 1, 'Obala', 'Setnja uz reku', 'Secret 2', 'obala.jpg', 45.251, 19.846),
((SELECT "Id" FROM "Tours"."Tours" WHERE "Name" = 'Dunavska staza'), 2, 'Vidikovac', 'Pogled na Dunav', 'Secret 3', 'vidikovac.jpg', 45.255, 19.852),
((SELECT "Id" FROM "Tours"."Tours" WHERE "Name" = 'Tarin izazov'), 1, 'Ulaz', 'Ulazna tacka', 'Secret 4', 'ulaz.jpg', 43.868, 19.901);
