CREATE TABLE IF NOT EXISTS Authors
(
    Id         SERIAL PRIMARY KEY,
    AuthorName TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS AuthorsSpecialties
(
    Id            SERIAL PRIMARY KEY,
    AuthorId      INT REFERENCES Authors (Id) ON DELETE CASCADE,
    SpecialtyName TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Inventory
(
    Id            SERIAL PRIMARY KEY,
    InventoryName TEXT NOT NULL UNIQUE,
    TotalAmount   INT  NOT NULL CHECK (TotalAmount >= 0)
);

CREATE TABLE IF NOT EXISTS Workers
(
    Id        SERIAL PRIMARY KEY,
    Name      TEXT NOT NULL,
    Specialty TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Artists
(
    Id    INT PRIMARY KEY REFERENCES Workers (Id) ON DELETE CASCADE,
    Grade TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Origins
(
    Id               SERIAL PRIMARY KEY,
    OriginName       TEXT NOT NULL,
    OriginComposerId INT REFERENCES Authors (Id) ON DELETE CASCADE,
    WriterId         INT REFERENCES Authors (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Staging
(
    Id                SERIAL PRIMARY KEY,
    Duration          INTERVAL NOT NULL,
    DirectorId        INT REFERENCES Workers (Id) ON DELETE CASCADE,
    StagingComposerId INT REFERENCES Workers (Id) ON DELETE CASCADE,
    OriginId          INT REFERENCES Origins (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS NeededInventory
(
    InventoryId INT REFERENCES Inventory (Id) ON DELETE CASCADE,
    StagingId   INT REFERENCES Staging (Id) ON DELETE CASCADE,
    Count       INT NOT NULL CHECK (Count > 0),
    PRIMARY KEY (InventoryId, StagingId)
);

CREATE TABLE IF NOT EXISTS Halls
(
    Id       SERIAL PRIMARY KEY,
    Capacity INT NOT NULL CHECK (Capacity > 0)
);

CREATE TABLE IF NOT EXISTS Roles
(
    Id        SERIAL PRIMARY KEY,
    StagingId INT REFERENCES Staging (Id) ON DELETE CASCADE,
    RoleName  TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Performances
(
    Id               SERIAL PRIMARY KEY,
    StartDateTime    TIMESTAMP NOT NULL,
    StagingId        INT REFERENCES Staging (Id) ON DELETE CASCADE,
    SoldTicketsCount INT       NOT NULL DEFAULT 0 CHECK (SoldTicketsCount >= 0),
    HallId           INT REFERENCES Halls (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ArtistsInPerformances
(
    RoleId        INT REFERENCES Roles (Id) ON DELETE CASCADE,
    ArtistId      INT REFERENCES Artists (Id) ON DELETE CASCADE,
    PerformanceId INT REFERENCES Performances (Id) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, ArtistId, PerformanceId)
);

CREATE TABLE IF NOT EXISTS Users
(
    Id             SERIAL PRIMARY KEY,
    Login          VARCHAR(100) NOT NULL UNIQUE,
    HashedPassword VARCHAR(256) NOT NULL,
    Role           VARCHAR(50)  NOT NULL
);
