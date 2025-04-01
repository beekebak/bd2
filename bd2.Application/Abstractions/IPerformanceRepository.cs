using bd2.Application.DTO;
using bd2.Core.PerformanceAggregate;

namespace bd2.Application.Abstractions;

public interface IPerformanceRepository : IGenericRepository<Performance>
{
    public void UpdateArtistInPerformance(int performanceId, int roleId, int artistId);
    public void MovePerformanceToHall(int performanceId, int newHallId);
    public void ReturnTicket(int performanceId);
    public void BuyTicket(int performanceId);
    public IEnumerable<BusyResources> GetBusyArtistsAndHalls(DateTime checkTime);
    public IEnumerable<Performance> FilterPerformances(PerformanceFilter filter);
}