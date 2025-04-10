CREATE INDEX idx_performances_start_date_time ON Performances (StartDateTime);
CREATE INDEX idx_performances_staging_id ON Performances (StagingId);
CREATE INDEX idx_performances_hall_id ON Performances (HallId);

CREATE INDEX idx_artists_in_performances_artist_id ON ArtistsInPerformances (ArtistId);
CREATE INDEX idx_artists_in_performances_performance_id ON ArtistsInPerformances (PerformanceId);
CREATE INDEX idx_artists_in_performances_role_id ON ArtistsInPerformances (RoleId);
CREATE INDEX idx_artists_in_performances_all ON ArtistsInPerformances (ArtistId, PerformanceId, RoleId);

CREATE INDEX idx_staging_origin_id ON Staging (OriginId);
CREATE INDEX idx_staging_director_id ON Staging (DirectorId);
CREATE INDEX idx_staging_staging_composer_id ON Staging (StagingComposerId);

CREATE INDEX idx_origins_origin_composer_id ON Origins (OriginComposerId);
CREATE INDEX idx_origins_writer_id ON Origins (WriterId);
CREATE INDEX idx_origins_origin_name ON Origins (OriginName);

CREATE INDEX idx_authors_author_name ON Authors (AuthorName);

CREATE INDEX idx_workers_name ON Workers (Name);
CREATE INDEX idx_workers_specialty ON Workers (Specialty);

CREATE INDEX idx_artists_grade ON Artists (Grade);

CREATE INDEX idx_inventory_inventory_name ON Inventory (InventoryName);

CREATE INDEX idx_needed_inventory_staging_id ON NeededInventory (StagingId);
CREATE INDEX idx_needed_inventory_inventory_id ON NeededInventory (InventoryId);
CREATE INDEX idx_needed_inventory_both ON NeededInventory (StagingId, InventoryId);

CREATE INDEX idx_roles_staging_id ON Roles (StagingId);