CREATE OR REPLACE VIEW AllPerformancesData AS
SELECT
    p.Id AS Id,
    p.StartDateTime AS StartDateTime,
    o.OriginName as OriginName,
    ac.AuthorName AS ComposerName,
    aw.AuthorName AS WriterName,
    wc.Name AS StagingComposerName,
    wd.Name AS DirectorName,
    w.Name AS ArtistName
FROM Performances p
         JOIN Staging s ON p.StagingId = s.Id
         JOIN Origins o ON s.OriginId = o.Id
         JOIN Authors ac ON o.OriginComposerId = ac.Id
         JOIN Authors aw ON o.WriterId = aw.Id
         JOIN Workers wc ON s.StagingComposerId = wc.Id
         JOIN Workers wd ON s.DirectorId = wd.Id
         LEFT JOIN ArtistsInPerformances aip ON p.Id = aip.PerformanceId
         LEFT JOIN Artists a ON aip.ArtistId = a.Id
         LEFT JOIN Workers w ON a.Id = w.Id;

CREATE OR REPLACE VIEW StagingDetails AS
SELECT
    s.Id AS Id,
    o.OriginName AS OriginName,
    aw.AuthorName AS WriterName,
    ac.AuthorName AS ComposerOriginName,
    wd.Name AS DirectorName,
    wc.Name AS ComposerName,
    s.Duration AS Duration
FROM Staging s
         JOIN Origins o ON s.OriginId = o.Id
         JOIN Workers wd ON s.DirectorId = wd.Id
         JOIN Workers wc ON s.StagingComposerId = wc.Id
         JOIN Authors aw ON o.WriterId = aw.Id
         JOIN Authors ac ON o.OriginComposerId = ac.Id;