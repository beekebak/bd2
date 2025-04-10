DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'director') THEN
        CREATE ROLE director;
    END IF;

    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'manager') THEN
        CREATE ROLE manager;
    END IF;

    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'storekeeper') THEN
        CREATE ROLE storekeeper;
    END IF;

    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'guest') THEN
        CREATE ROLE guest;
    END IF;

    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Authors TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE AuthorsSpecialties TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Inventory TO director, storekeeper;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Workers TO director;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Artists TO director;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Origins TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Staging TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE NeededInventory TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Halls TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Roles TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Performances TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE ArtistsInPerformances TO director, manager;
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE Users TO director;

    GRANT SELECT, UPDATE, DELETE ON TABLE Workers TO manager;
    GRANT SELECT, UPDATE, DELETE ON TABLE Artists TO manager;
    
    GRANT SELECT, UPDATE ON TABLE Users TO manager, storekeeper;
    GRANT SELECT ON TABLE Users TO guest;

    GRANT SELECT ON TABLE Halls TO guest;
    GRANT SELECT ON TABLE Staging TO guest;
    GRANT SELECT, UPDATE ON TABLE Performances TO guest;

    GRANT USAGE, SELECT ON SEQUENCE Authors_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE AuthorsSpecialties_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE Inventory_Id_seq TO director, storekeeper;
    GRANT USAGE, SELECT ON SEQUENCE Workers_Id_seq TO director;
    GRANT USAGE, SELECT ON SEQUENCE Origins_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE Staging_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE Halls_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE Roles_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE Performances_Id_seq TO director, manager;
    GRANT USAGE, SELECT ON SEQUENCE Users_Id_seq TO director;

    GRANT EXECUTE ON FUNCTION get_inventory_usage(p_check_time TIMESTAMP) TO director, manager, storekeeper;
    GRANT EXECUTE ON FUNCTION get_inventory_usage_interval(p_start_time TIMESTAMP, p_duration INTERVAL) TO director, manager, storekeeper;
    GRANT EXECUTE ON FUNCTION get_busy_halls(p_check_time TIMESTAMP) TO director, manager;
    GRANT EXECUTE ON FUNCTION get_busy_halls_interval(p_start_time TIMESTAMP, p_duration INTERVAL) TO director, manager;
    GRANT EXECUTE ON FUNCTION get_busy_artists(p_check_time TIMESTAMP) TO director, manager;
    GRANT EXECUTE ON FUNCTION get_busy_artists_interval(p_start_time TIMESTAMP, p_duration INTERVAL) TO director, manager;
    GRANT EXECUTE ON FUNCTION check_possible_Performances(p_staging_id INT, p_start_time TIMESTAMP, p_duration INTERVAL, p_hall_id INT, p_artist_ids INT[]) TO director, manager;
    GRANT EXECUTE ON PROCEDURE buy_ticket(p_performance_id INT) TO director, guest;
    GRANT EXECUTE ON PROCEDURE return_ticket(performance_id INT) TO director, guest;
    GRANT EXECUTE ON FUNCTION move_performance_to_hall(p_performance_id INT, p_new_hall_id INT) TO director, manager;
    GRANT EXECUTE ON FUNCTION update_artist_in_performance(p_performance_id INT, p_role_id INT, p_artist_id INT) TO director, manager;
    GRANT EXECUTE ON FUNCTION add_worker(p_name TEXT, p_specialty TEXT) TO director;
    GRANT EXECUTE ON FUNCTION add_artist(p_name TEXT, p_grade TEXT) TO director;
    GRANT EXECUTE ON FUNCTION add_performance(p_start_date_time TIMESTAMP, p_staging_id INT, p_hall_id INT, p_artists_data JSONB) TO director, manager;
    GRANT EXECUTE ON FUNCTION add_staging_with_inventory_roles(p_duration INTERVAL, p_director_id INT, p_staging_composer_id INT, p_origin_id INT, p_inventory JSONB, p_roles JSONB) TO director, manager;
    GRANT EXECUTE ON FUNCTION update_staging_with_inventory_roles(p_staging_id INT, p_duration INTERVAL, p_director_id INT, p_staging_composer_id INT, p_origin_id INT, p_inventory JSONB, p_roles JSONB) TO director, manager;

    GRANT SELECT ON TABLE AllPerformancesData TO guest;
    GRANT SELECT ON TABLE StagingDetails TO guest;
    
    GRANT director TO myuser;
    GRANT guest TO myuser;
    GRANT storekeeper TO myuser;
    GRANT manager TO myuser;
END;
$$ LANGUAGE plpgsql;
