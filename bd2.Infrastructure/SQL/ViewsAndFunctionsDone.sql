CREATE OR REPLACE VIEW AllPerformancesData AS
SELECT
    p.PerformanceId,
    p.StartDateTime,
    o.OriginName,
    ac.AuthorName AS ComposerName,
    aw.AuthorName AS WriterName,
    wc.Name AS StagingComposerName,
    wd.Name AS DirectorName,
    w.Name AS ArtistName
FROM Performances p
         JOIN Staging s ON p.StagingId = s.StagingId
         JOIN Origins o ON s.OriginId = o.OriginId
         LEFT JOIN Authors ac ON o.OriginComposerId = ac.AuthorId
         LEFT JOIN Authors aw ON o.WriterId = aw.AuthorId
         LEFT JOIN Workers wc ON s.StagingComposerId = wc.WorkerId
         LEFT JOIN Workers wd ON s.DirectorId = wd.WorkerId
         LEFT JOIN ArtistsInPerformances aip ON p.PerformanceId = aip.PerformanceId
         LEFT JOIN Artists a ON aip.ArtistId = a.ArtistId
         LEFT JOIN Workers w ON a.ArtistId = w.WorkerId;

CREATE OR REPLACE FUNCTION get_available_inventory(p_check_time TIMESTAMP)
    RETURNS TABLE (
                      InventoryId INT,
                      InventoryName TEXT,
                      AvailableAmount INT
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT
            i.InventoryId,
            i.InventoryName,
            i.TotalAmount - COALESCE(u.TotalInUse, 0) AS AvailableAmount
        FROM Inventory i
                 LEFT JOIN (
            SELECT InventoryId, SUM(TotalInUse) AS TotalInUse
            FROM get_inventory_usage(p_check_time)
            GROUP BY InventoryId
        ) u ON i.InventoryId = u.InventoryId;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_busy_artists_and_halls(p_check_time TIMESTAMP)
    RETURNS TABLE (
                      HallId INT,
                      ArtistId INT
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT
            p.HallId,
            aip.ArtistId
        FROM Performances p
                 JOIN Staging s ON p.StagingId = s.StagingId
                 LEFT JOIN ArtistsInPerformances aip ON p.PerformanceId = aip.PerformanceId
        WHERE p.StartDateTime <= p_check_time AND (p.StartDateTime + s.Duration) > p_check_time;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE VIEW StagingDetails AS
SELECT
    s.StagingId,
    o.OriginName,
    aw.AuthorName AS WriterName,
    ac.AuthorName AS ComposerOriginName,
    wd.Name AS DirectorName,
    wc.Name AS ComposerName,
    s.Duration
FROM Staging s
         JOIN Origins o ON s.OriginId = o.OriginId
         LEFT JOIN Workers wd ON s.DirectorId = wd.WorkerId
         LEFT JOIN Workers wc ON s.StagingComposerId = wc.WorkerId
         LEFT JOIN Authors aw ON o.WriterId = aw.AuthorId
         LEFT JOIN Authors ac ON o.OriginComposerId = ac.AuthorId;