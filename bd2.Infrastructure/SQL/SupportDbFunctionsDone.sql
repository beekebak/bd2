CREATE OR REPLACE FUNCTION get_inventory_usage(p_check_time TIMESTAMP)
    RETURNS TABLE (InventoryId INT, InventoryName TEXT, TotalInUse INT)
AS $$
BEGIN
    RETURN QUERY
        SELECT ni.InventoryId, i.InventoryName, SUM(ni.Count) AS TotalInUse
        FROM Performances p
                 JOIN Staging s ON p.StagingId = s.StagingId
                 JOIN NeededInventory ni ON s.StagingId = ni.StagingId
                 JOIN Inventory i ON ni.InventoryId = i.InventoryId
        WHERE p.StartDateTime <= p_check_time
          AND (p.StartDateTime + s.Duration) > p_check_time
        GROUP BY ni.InventoryId, i.InventoryName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_inventory_usage_interval(p_start_time TIMESTAMP, p_duration INTERVAL)
    RETURNS TABLE (InventoryId INT, InventoryName TEXT, TotalInUse INT)
AS $$
BEGIN
    RETURN QUERY
        SELECT ni.InventoryId, i.InventoryName, SUM(ni.Count) AS TotalInUse
        FROM Performances p
                 JOIN Staging s ON p.StagingId = s.StagingId
                 JOIN NeededInventory ni ON s.StagingId = ni.StagingId
                 JOIN Inventory i ON ni.InventoryId = i.InventoryId
        WHERE
            (
                (p.StartDateTime < p_start_time + p_duration AND (p.StartDateTime + s.Duration) > p_start_time)
                    OR
                (p.StartDateTime >= p_start_time AND p.StartDateTime < p_start_time + p_duration)
                    OR
                ((p.StartDateTime < p_start_time) AND (p.StartDateTime + s.Duration) > (p_start_time + p_duration))
                )
        GROUP BY ni.InventoryId, i.InventoryName;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION get_busy_halls(p_check_time TIMESTAMP)
    RETURNS TABLE (HallId INT, HallCapacity INT)
AS $$
BEGIN
    RETURN QUERY
        SELECT DISTINCT h.HallId, h.Capacity
        FROM Performances p
                 JOIN Halls h ON p.HallId = h.HallId
                 JOIN Staging s ON p.StagingId = s.StagingId
        WHERE p.StartDateTime <= p_check_time
          AND (p.StartDateTime + s.Duration) > p_check_time;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_busy_halls_interval(p_start_time TIMESTAMP, p_duration INTERVAL)
    RETURNS TABLE (HallId INT, HallCapacity INT)
AS $$
BEGIN
    RETURN QUERY
        SELECT DISTINCT h.HallId, h.Capacity
        FROM Performances p
                 JOIN Halls h ON p.HallId = h.HallId
                 JOIN Staging s ON p.StagingId = s.StagingId
        WHERE
            (
                (p.StartDateTime < p_start_time + p_duration AND (p.StartDateTime + s.Duration) > p_start_time)
                    OR
                (p.StartDateTime >= p_start_time AND p.StartDateTime < p_start_time + p_duration)
                    OR
                ((p.StartDateTime < p_start_time) AND (p.StartDateTime + s.Duration) > (p_start_time + p_duration))
                );
END;
$$ LANGUAGE plpgsql;    

CREATE OR REPLACE FUNCTION get_busy_artists(p_check_time TIMESTAMP)
    RETURNS TABLE (ArtistId INT, Grade TEXT)
AS $$
BEGIN
    RETURN QUERY
        SELECT DISTINCT a.ArtistId, a.Grade
        FROM ArtistsInPerformances aip
                 JOIN Performances p ON aip.PerformanceId = p.PerformanceId
                 JOIN Staging s ON p.StagingId = s.StagingId
                 JOIN Artists a ON aip.ArtistId = a.ArtistId
        WHERE p.StartDateTime <= p_check_time
          AND (p.StartDateTime + s.Duration) > p_check_time;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_busy_artists_interval(p_start_time TIMESTAMP, p_duration INTERVAL)
    RETURNS TABLE (ArtistId INT, Grade TEXT)
AS $$
BEGIN
    RETURN QUERY
        SELECT DISTINCT a.ArtistId, a.Grade
        FROM ArtistsInPerformances aip
                 JOIN Performances p ON aip.PerformanceId = p.PerformanceId
                 JOIN Staging s ON p.StagingId = s.StagingId
                 JOIN Artists a ON aip.ArtistId = a.ArtistId
        WHERE
            (                
                (p.StartDateTime < p_start_time + p_duration AND (p.StartDateTime + s.Duration) > p_start_time)
                    OR
                (p.StartDateTime >= p_start_time AND p.StartDateTime < p_start_time + p_duration)
                    OR
                ((p.StartDateTime < p_start_time) AND (p.StartDateTime + s.Duration) > (p_start_time + p_duration))
                );
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION check_possible_Performances(
    p_staging_id INT,
    p_start_time TIMESTAMP,
    p_duration INTERVAL,
    p_hall_id INT,
    p_artist_ids INT[]
)
    RETURNS BOOLEAN AS $$
DECLARE
    is_hall_busy BOOLEAN;
    is_artists_busy BOOLEAN;
    roles_count INT;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM get_busy_halls_interval(p_start_time, p_duration) h
        WHERE h.HallId = p_hall_id
    ) INTO is_hall_busy;

    IF is_hall_busy THEN
        RAISE EXCEPTION 'Зал занят в указанный момент времени';
    END IF;

    SELECT EXISTS (
        SELECT 1
        FROM get_busy_artists_interval(p_start_time, p_duration) a
        WHERE a.ArtistId = ANY(p_artist_ids)
    ) INTO is_artists_busy;

    IF is_artists_busy THEN
        RAISE EXCEPTION 'Один или несколько артистов заняты в указанный момент времени';
    END IF;

    SELECT COUNT(*) INTO roles_count
    FROM Roles r
    WHERE r.StagingId = stagingid;

    IF roles_count != (SELECT array_length(p_artist_ids, 1)) THEN
        RAISE EXCEPTION 'Количество артистов не соответствует количеству ролей в постановке';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
                 SELECT ni.InventoryId,
                        SUM(ni.Count) AS TotalNeeded,
                        i.TotalAmount
                 FROM NeededInventory ni
                          JOIN Inventory i ON ni.InventoryId = i.InventoryId
                 WHERE ni.StagingId = p_staging_id
                 GROUP BY ni.InventoryId, i.TotalAmount
             ) needed
                 JOIN (
            SELECT InventoryId, SUM(TotalInUse) AS TotalUsed
            FROM get_inventory_usage_interval(p_start_time, p_duration)
            GROUP BY InventoryId
        ) used ON needed.InventoryId = used.InventoryId
        WHERE needed.TotalNeeded + COALESCE(used.TotalUsed, 0) > needed.TotalAmount
    ) THEN
        RAISE EXCEPTION 'Не хватает инвентаря для постановки';
    END IF;

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;
