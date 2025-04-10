CREATE OR REPLACE PROCEDURE buy_ticket(p_performance_id INT)
    LANGUAGE plpgsql
AS $$
DECLARE
    hall_capacity INT;
    sold_tickets  INT;
BEGIN
    BEGIN
        SELECT h.Capacity, p.SoldTicketsCount
        INTO hall_capacity, sold_tickets
        FROM Performances p
                 JOIN Halls h ON p.HallId = h.Id
        WHERE p.Id = p_performance_id;

        IF sold_tickets >= hall_capacity THEN
            RAISE EXCEPTION 'Места на постановку закончились!';
        END IF;

        UPDATE Performances
        SET SoldTicketsCount = SoldTicketsCount + 1
        WHERE Id = p_performance_id;

    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END;
END;
$$;

CREATE OR REPLACE PROCEDURE return_ticket(performance_id INT)
    LANGUAGE plpgsql
AS $$
BEGIN
    BEGIN
        UPDATE Performances
        SET SoldTicketsCount = SoldTicketsCount - 1
        WHERE Id = performance_id;
    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END;
END;
$$;

CREATE OR REPLACE FUNCTION move_performance_to_hall(p_performance_id INT, p_new_hall_id INT)
    RETURNS VOID AS
$$
DECLARE
    performance_start_time TIMESTAMP;
    performance_duration   INTERVAL;
    sold_tickets_count     INT;
    new_hall_capacity      INT;
BEGIN
    SELECT p.StartDateTime, s.Duration, p.SoldTicketsCount
    INTO performance_start_time, performance_duration, sold_tickets_count
    FROM Performances p
             JOIN Staging s ON p.StagingId = s.Id
    WHERE p.Id = p_performance_id;

    SELECT h.Capacity
    INTO new_hall_capacity
    FROM Halls h
    WHERE h.Id = p_new_hall_id;

    IF EXISTS (SELECT 1
               FROM get_busy_halls_interval(performance_start_time, performance_duration)
               WHERE HallId = p_new_hall_id) THEN
        RAISE EXCEPTION 'Новый зал занят в это время.';
    END IF;

    IF sold_tickets_count > new_hall_capacity THEN
        RAISE EXCEPTION 'Вместимость нового зала меньше количества проданных билетов.';
    END IF;

    UPDATE Performances
    SET HallId = p_new_hall_id
    WHERE Id = p_performance_id;

EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_artist_in_performance(p_performance_id INT, p_role_id INT, p_artist_id INT)
    RETURNS VOID AS
$$
DECLARE
    performance_start_time TIMESTAMP;
    performance_duration   INTERVAL;
BEGIN

    SELECT p.StartDateTime, s.Duration
    INTO performance_start_time, performance_duration
    FROM Performances p
             JOIN Staging s ON p.StagingId = s.Id
    WHERE p.Id = p_performance_id;

    IF EXISTS (SELECT 1
               FROM get_busy_artists_interval(performance_start_time, performance_duration)
               WHERE ArtistId = p_artist_id) THEN
        RAISE EXCEPTION 'Артист занят в это время.';
    END IF;

    UPDATE ArtistsInPerformances
    SET ArtistId = p_artist_id
    WHERE PerformanceId = p_performance_id
      AND RoleId = p_role_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_worker(p_name TEXT, p_specialty TEXT)
    RETURNS INT AS
$$
DECLARE
    new_worker_id INT;
BEGIN
    IF p_specialty = 'Актер' THEN
        RAISE EXCEPTION 'Нельзя добавлять работников с специальностью "Актер" через эту функцию.';
    END IF;

    INSERT INTO Workers (Name, Specialty)
    VALUES (p_name, p_specialty)
    RETURNING Id INTO new_worker_id;
    RETURN new_worker_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_artist(p_name TEXT, p_grade TEXT)
    RETURNS INT AS
$$
DECLARE
    new_worker_id INT;
BEGIN

    INSERT INTO Workers (Name, Specialty)
    VALUES (p_name, 'Актер')
    RETURNING Id INTO new_worker_id;

    INSERT INTO Artists (Id, Grade)
    VALUES (new_worker_id, p_grade);
    RETURN new_worker_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_performance(
    p_start_date_time TIMESTAMP,
    p_staging_id INT,
    p_hall_id INT,
    p_artists_data JSONB
)
    RETURNS INT AS
$$
DECLARE
    staging_duration INTERVAL;
    artist_record    JSONB;
    artist_id        INT;
    perf_id          INT;
BEGIN
    SELECT Duration
    INTO staging_duration
    FROM Staging
    WHERE Id = p_staging_id;

    IF staging_duration IS NULL THEN
        RAISE EXCEPTION 'Постановка не найдена.';
    END IF;

    IF EXISTS (SELECT 1
               FROM get_busy_halls_interval(p_start_date_time, staging_duration)AS h(HallId, HallCapacity)
               WHERE HallId = p_hall_id) THEN
        RAISE EXCEPTION 'Зал занят в это время.';
    END IF;

    IF EXISTS (SELECT 1
               FROM get_inventory_usage_interval(p_start_date_time, staging_duration) AS inv(InventoryId, InventoryName, TotalInUse)
               WHERE InventoryId IN (SELECT InventoryId FROM NeededInventory WHERE StagingId = p_staging_id)) THEN
        RAISE EXCEPTION 'Необходимый инвентарь занят в это время.';
    END IF;

    FOR artist_record IN SELECT * FROM jsonb_array_elements(p_artists_data)
        LOOP
            artist_id := (artist_record ->> 'artist_id')::INT;
            IF EXISTS (SELECT 1
                       FROM get_busy_artists_interval(p_start_date_time, staging_duration)
                       WHERE ArtistId = artist_id) THEN
                RAISE EXCEPTION 'Один или несколько артистов заняты в это время.';
            END IF;
        END LOOP;

    INSERT INTO Performances (StartDateTime, StagingId, HallId)
    VALUES (p_start_date_time, p_staging_id, p_hall_id)
    RETURNING Id INTO perf_id;

    INSERT INTO ArtistsInPerformances (ArtistId, RoleId, PerformanceId)
    SELECT
        (elem ->> 'artist_id')::INT,
        (elem ->> 'role_id')::INT,
        perf_id
    FROM jsonb_array_elements(p_artists_data) AS elem;
    RETURN perf_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_staging_with_inventory_roles(
    p_duration INTERVAL,
    p_director_id INT,
    p_staging_composer_id INT,
    p_origin_id INT,
    p_inventory JSONB,
    p_roles JSONB
) RETURNS INT AS
$$
DECLARE
    v_staging_id INT;
    item         JSONB;
    role_item    JSONB;
BEGIN

    INSERT INTO Staging (Duration, DirectorId, StagingComposerId, OriginId)
    VALUES (p_duration, p_director_id, p_staging_composer_id, p_origin_id)
    RETURNING Id INTO v_staging_id;

    FOR item IN SELECT * FROM jsonb_array_elements(p_inventory)
        LOOP
            INSERT INTO NeededInventory (StagingId, InventoryId, Count)
            VALUES (v_staging_id, (item ->> 'inventory_id')::INT, (item ->> 'count')::INT);
        END LOOP;

    FOR role_item IN SELECT * FROM jsonb_array_elements(p_roles)
        LOOP
            INSERT INTO Roles (StagingId, RoleName)
            VALUES (v_staging_id, role_item ->> 'role_name');
        END LOOP;

    RETURN v_staging_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_staging_with_inventory_roles(
    p_staging_id INT,
    p_duration INTERVAL,
    p_director_id INT,
    p_staging_composer_id INT,
    p_origin_id INT,
    p_inventory JSONB,
    p_roles JSONB
) RETURNS VOID AS
$$
DECLARE
    item      JSONB;
    role_item JSONB;
BEGIN

    UPDATE Staging
    SET Duration          = p_duration,
        DirectorId        = p_director_id,
        StagingComposerId = p_staging_composer_id,
        OriginId          = p_origin_id
    WHERE Id = p_staging_id;

    DELETE FROM NeededInventory WHERE StagingId = p_staging_id;

    FOR item IN SELECT * FROM jsonb_array_elements(p_inventory)
        LOOP
            INSERT INTO NeededInventory (StagingId, InventoryId, Count)
            VALUES (p_staging_id, (item ->> 'inventory_id')::INT, (item ->> 'count')::INT);
        END LOOP;

    DELETE FROM Roles WHERE StagingId = p_staging_id;

    FOR role_item IN SELECT * FROM jsonb_array_elements(p_roles)
        LOOP
            INSERT INTO Roles (StagingId, RoleName)
            VALUES (p_staging_id, role_item ->> 'role_name');
        END LOOP;

EXCEPTION
    WHEN OTHERS THEN
        RAISE;
END;
$$ LANGUAGE plpgsql;