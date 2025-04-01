using bd2.Core.StagingAggregate;

namespace bd2.Core.PerformanceAggregate;

public class Performance(
    int id,
    DateTime startDate,
    Staging staging,
    Hall hall,
    int soldTicketsCount,
    List<Performance.ArtistsInPerformance> artists)
{
    public int Id { get; private set; } = id;
    public DateTime StartDate { get; private set; } = startDate;
    public Staging StagingId { get; private set; } = staging;
    public Hall Hall { get; private set; } = hall;
    public int SoldTicketsCount { get; private set; } = soldTicketsCount;
    public List<ArtistsInPerformance> Artists { get; private set; } = artists;
    public record ArtistsInPerformance(int ArtistId, int RoleId);
}
