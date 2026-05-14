INSERT INTO "Tours"."Tours" ("AuthorId", "Name", "Description", "Difficulty", "Tags", "Status", "Price")
VALUES
(101, 'Fruska Gora test ruta', 'Seed tura za query testove.', 'Easy', '["planina", "vikend"]'::jsonb, 'Draft', 0),
(101, 'Dunavska staza', 'Jos jedna seed tura autora 101.', 'Medium', '["reka"]'::jsonb, 'Draft', 0),
(202, 'Tarin izazov', 'Seed tura drugog autora.', 'Hard', '["avantura"]'::jsonb, 'Draft', 0);
