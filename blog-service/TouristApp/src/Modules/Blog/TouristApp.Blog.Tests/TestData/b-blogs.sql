INSERT INTO blog."Blogs" ("Id", "Title", "Description", "CreationDate", "Images") VALUES
                                                                             (1, 'Izlet na Taru',      '**Prelepo** iskustvo u srcu šume.',             '2026-01-10 08:00:00', '["tara1.jpg","tara2.jpg"]'),
                                                                             (2, 'Beograd noću',       'Grad koji nikad ne spava. *Preporučujem!*',     '2026-02-14 20:00:00', '["bg1.jpg"]'),
                                                                             (3, 'Blog bez komentara', 'Ovaj blog nema komentara — koristi se za test.','2026-03-01 12:00:00', '[]');

-- Blog 1 ima 2 komentara (različiti autori)
INSERT INTO blog."comments" ("Id", "BlogId", "AuthorId", "Text", "CreatedAt", "LastModifiedAt") VALUES
                                                                                            (100, 1, 101, 'Odličan opis!',                          '2026-01-11 09:00:00', NULL),
                                                                                            (101, 1, 102, 'I ja sam bio tu, prelepо mjesto.',        '2026-01-12 10:30:00', NULL);

-- Blog 2 ima 1 komentar koji je izmenjen
INSERT INTO blog."comments" ("Id", "BlogId", "AuthorId", "Text", "CreatedAt", "LastModifiedAt") VALUES
    (102, 2, 101, 'Izmenjeni komentar — originalni tekst.', '2026-02-15 08:00:00', '2026-02-16 14:00:00');