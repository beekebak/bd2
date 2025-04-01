using System.ComponentModel.DataAnnotations.Schema;

namespace bd2.Infrastructure.DTO;

[Table("Authors")]
public class AuthorDto
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; }
}

[Table("AuthorsSpecialties")]
public class AuthorsSpecialtyDto
{
    public int AuthorSpecialtyId { get; set; }
    public int AuthorId { get; set; }
    public string SpecialtyName { get; set; }
}


[Table("Stagings")]
public class StagingDto
{
    public int StagingId { get; set; }
    public TimeSpan Duration { get; set; }
    public int DirectorId { get; set; }
    public int StagingComposerId { get; set; }
    public int OriginId { get; set; }
}

[Table("Performances")]
public class PerformanceDto
{
    public int PerformanceId { get; set; }
    public DateTime StartDateTime { get; set; }
    public int StagingId { get; set; }
    public int HallId { get; set; }
    public int SoldTicketsCount { get; set; }
}

[Table("Origins")]
public class OriginDto
{
    public int OriginId { get; set; }
    public string OriginName { get; set; }
    public int OriginComposerId { get; set; }
    public int WriterId { get; set; }
}

[Table("Workers")]
public class WorkerDto
{
    public int WorkerId { get; set; }
    public string Name { get; set; }
    public string Specialty { get; set; }
}

[Table("Artists")]
public class ArtistDto
{
    public int ArtistId { get; set; }
    public string Grade { get; set; }
}

[Table("Roles")]
public class RoleDto
{
    public int RoleId { get; set; }
    public int StagingId { get; set; }
    public string RoleName { get; set; }
}

[Table("ArtistsInPerformances")]
public class ArtistsInPerformanceDto
{
    public int PerformanceId { get; set; }
    public int ArtistId { get; set; }
    public int RoleId { get; set; }
}

[Table("Inventory")]
public class InventoryDto
{
    public int InventoryId { get; set; }
    public string InventoryName { get; set; }
    public int TotalAmount { get; set; }
}

[Table("NeededInventory")]
public class NeededInventoryDto
{
    public int StagingId { get; set; }
    public int InventoryId { get; set; }
    public int Count { get; set; }
}

[Table("Halls")]
public class HallDto
{
    public int HallId { get; set; }
    public int Capacity { get; set; }
}

public class PerformanceIdWrapper
{
    public int PerformanceId { get; set; }
}