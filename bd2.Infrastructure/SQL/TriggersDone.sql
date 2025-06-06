CREATE OR REPLACE FUNCTION cancel_performances_on_artist_dismissal() RETURNS TRIGGER AS $$
BEGIN
    DELETE FROM Performances
    WHERE Id IN (
        SELECT p.Id
        FROM Performances p
                 LEFT JOIN ArtistsInPerformances aip ON p.Id = aip.PerformanceId
        WHERE aip.ArtistId = OLD.ID
    );

    DELETE FROM ArtistsInPerformances
    WHERE ArtistId = OLD.Id;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_cancel_performances
    BEFORE DELETE ON Artists
    FOR EACH ROW EXECUTE FUNCTION cancel_performances_on_artist_dismissal();

CREATE OR REPLACE FUNCTION check_performance_artists()
    RETURNS TRIGGER AS $$
DECLARE
    performance_id INT;
    total_count INT;
    trainee_count INT;
BEGIN
    FOR performance_id IN
        SELECT DISTINCT PerformanceId FROM ArtistsInPerformances
        LOOP
            SELECT COUNT(*) INTO total_count
            FROM ArtistsInPerformances aip
                     JOIN Artists a ON a.Id = aip.ArtistId
            WHERE aip.PerformanceId = performance_id;

            SELECT COUNT(*) INTO trainee_count
            FROM ArtistsInPerformances aip
                     JOIN Artists a ON a.Id = aip.ArtistId
            WHERE aip.PerformanceId = performance_id
              AND a.Grade = 'Стажер';

            IF total_count > 0 AND trainee_count = total_count THEN
                RAISE EXCEPTION 'Постановка % не может состоять только из стажёров.', performance_id;
            END IF;
        END LOOP;

    RETURN NULL; 
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_performances_before_staging_update
    AFTER UPDATE ON ArtistsInPerformances
    FOR EACH STATEMENT
EXECUTE FUNCTION check_performance_artists();

CREATE TRIGGER trg_check_performances_after_staging_insert
    AFTER INSERT ON ArtistsInPerformances
    FOR EACH STATEMENT
EXECUTE FUNCTION check_performance_artists();

CREATE OR REPLACE FUNCTION check_performances_before_staging_update()
    RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM Performances WHERE StagingId = OLD.Id) THEN
        RAISE EXCEPTION 'Нельзя обновить постановку, для которой уже есть выступления.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_performances_before_staging_update
    BEFORE UPDATE ON Staging
    FOR EACH ROW
EXECUTE FUNCTION check_performances_before_staging_update();

CREATE OR REPLACE FUNCTION check_inventory_usage_before_update()
    RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM Performances p
                 JOIN Staging s ON p.StagingId = s.Id
                 JOIN NeededInventory ni ON s.Id = ni.StagingId
        WHERE ni.InventoryId = OLD.Id
    ) THEN
        RAISE EXCEPTION 'Нельзя изменить количество инвентаря, если он используется в постановках.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_inventory_usage_before_update
    BEFORE UPDATE OF TotalAmount ON Inventory
    FOR EACH ROW
EXECUTE FUNCTION check_inventory_usage_before_update();

CREATE OR REPLACE FUNCTION check_needed_inventory_usage_before_update()
    RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM Performances p
                 JOIN Staging s ON p.StagingId = s.Id
        WHERE s.Id = OLD.StagingId
    ) THEN
        RAISE EXCEPTION 'Нельзя изменить количество необходимого инвентаря, если есть связанные выступления.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_needed_inventory_usage_before_update
    BEFORE UPDATE OF Count ON NeededInventory
    FOR EACH ROW
EXECUTE FUNCTION check_needed_inventory_usage_before_update();

CREATE OR REPLACE FUNCTION check_author_specialties()
    RETURNS TRIGGER AS $$
DECLARE
    composer_specialty TEXT;
    writer_specialty TEXT;
BEGIN
    SELECT SpecialtyName INTO composer_specialty
    FROM AuthorsSpecialties
    WHERE AuthorId = NEW.OriginComposerId AND SpecialtyName = 'Композитор';

    SELECT SpecialtyName INTO writer_specialty
    FROM AuthorsSpecialties
    WHERE AuthorId = NEW.WriterId AND SpecialtyName = 'Писатель';

    IF composer_specialty IS NULL THEN
        RAISE EXCEPTION 'Композитор должен иметь специальность "Композитор".';
    END IF;

    IF writer_specialty IS NULL THEN
        RAISE EXCEPTION 'Писатель должен иметь специальность "Писатель".';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_author_specialties_insert_update
    BEFORE INSERT OR UPDATE ON Origins
    FOR EACH ROW
EXECUTE FUNCTION check_author_specialties();

CREATE OR REPLACE FUNCTION check_worker_specialties_staging()
    RETURNS TRIGGER AS $$
DECLARE
    composer_specialty TEXT;
    director_specialty TEXT;
BEGIN
    SELECT Specialty INTO composer_specialty
    FROM Workers
    WHERE Id = NEW.StagingComposerId;

    SELECT Specialty INTO director_specialty
    FROM Workers
    WHERE Id = NEW.DirectorId;

    IF composer_specialty != 'Композитор' THEN
        RAISE EXCEPTION 'Композитор постановки должен иметь специальность "Композитор".';
    END IF;

    IF director_specialty != 'Режиссер' THEN
        RAISE EXCEPTION 'Режиссер постановки должен иметь специальность "Режиссер".';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_worker_specialties_staging
    BEFORE INSERT OR UPDATE ON Staging
    FOR EACH ROW
EXECUTE FUNCTION check_worker_specialties_staging();

CREATE OR REPLACE FUNCTION check_hall_usage_before_update()
    RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM Performances
        WHERE HallId = OLD.Id
    ) THEN
        RAISE EXCEPTION 'Нельзя обновить зал, если есть связанные выступления.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_hall_usage_before_update
    BEFORE UPDATE ON Halls
    FOR EACH ROW
EXECUTE FUNCTION check_hall_usage_before_update();

CREATE OR REPLACE FUNCTION check_inventory_deletion()
    RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM NeededInventory
        WHERE InventoryId = OLD.Id
    ) THEN
        RAISE EXCEPTION 'Нельзя удалить инвентарь, который используется в постановках.';
    END IF;
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_inventory_deletion
    BEFORE DELETE ON Inventory
    FOR EACH ROW
EXECUTE FUNCTION check_inventory_deletion();