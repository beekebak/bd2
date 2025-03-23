namespace bd2.Core.PerformanceAggregate;

public class Performance(
    long id,
    DateTime startDate,
    int stagingId,
    Hall hall,
    int soldTicketsCount,
    List<Performance.ArtistsInPerformance> artists)
{
    public long Id { get; private set; } = id;
    public DateTime StartDate { get; private set; } = startDate;
    public int StagingId { get; private set; } = stagingId;
    public Hall Hall { get; private set; } = hall;
    public int SoldTicketsCount { get; private set; } = soldTicketsCount;
    public List<ArtistsInPerformance> Artists { get; private set; } = artists;

    public struct ArtistsInPerformance(int ArtistId, int RoleId);
}
