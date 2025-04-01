CREATE TABLE IF NOT EXISTS Authors
(
    AuthorId   SERIAL PRIMARY KEY,
    AuthorName TEXT NOT NULL UNIQUE 
);

CREATE TABLE IF NOT EXISTS AuthorsSpecialties
(
    AuthorSpecialtyId SERIAL PRIMARY KEY,
    AuthorId          INT REFERENCES Authors (AuthorId) ON DELETE CASCADE,
    SpecialtyName     TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Inventory
(
    InventoryId   SERIAL PRIMARY KEY,
    InventoryName TEXT NOT NULL UNIQUE,
    TotalAmount   INT  NOT NULL CHECK (TotalAmount >= 0)
);

CREATE TABLE IF NOT EXISTS Workers
(
    WorkerId  SERIAL PRIMARY KEY,
    Name      TEXT NOT NULL,
    Specialty TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Artists
(
    ArtistId INT PRIMARY KEY REFERENCES Workers (WorkerId) ON DELETE CASCADE,
    Grade    TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Origins
(
    OriginId         SERIAL PRIMARY KEY,
    OriginName       TEXT NOT NULL,
    OriginComposerId INT REFERENCES Authors (AuthorId) ON DELETE CASCADE,
    WriterId         INT REFERENCES Authors (AuthorId) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Staging
(
    StagingId         SERIAL PRIMARY KEY,
    Duration          INTERVAL NOT NULL,
    DirectorId        INT REFERENCES Workers (WorkerId) ON DELETE CASCADE,
    StagingComposerId INT REFERENCES Workers (WorkerId) ON DELETE CASCADE,
    OriginId          INT REFERENCES Origins (OriginId) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS NeededInventory
(
    InventoryId INT REFERENCES Inventory (InventoryId) ON DELETE CASCADE,
    StagingId   INT REFERENCES Staging (StagingId) ON DELETE CASCADE,
    Count       INT NOT NULL CHECK (Count > 0),
    PRIMARY KEY (InventoryId, StagingId)
);

CREATE TABLE IF NOT EXISTS Halls
(
    HallId   SERIAL PRIMARY KEY,
    Capacity INT NOT NULL CHECK (Capacity > 0)
);

CREATE TABLE IF NOT EXISTS Roles
(
    RoleId    SERIAL PRIMARY KEY,
    StagingId INT REFERENCES Staging (StagingId) ON DELETE CASCADE,
    RoleName  TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Performances
(
    PerformanceId    SERIAL PRIMARY KEY,
    StartDateTime    TIMESTAMP NOT NULL,
    StagingId        INT REFERENCES Staging (StagingId) ON DELETE CASCADE,
    SoldTicketsCount INT NOT NULL DEFAULT 0 CHECK (SoldTicketsCount >= 0),
    HallId           INT REFERENCES Halls (HallId) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ArtistsInPerformances
(
    RoleId        INT REFERENCES Roles (RoleId) ON DELETE CASCADE,
    ArtistId      INT REFERENCES Artists (ArtistId) ON DELETE CASCADE,
    PerformanceId INT REFERENCES Performances (PerformanceId) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, ArtistId, PerformanceId)
);