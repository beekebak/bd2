using bd2.Application.DTO;
using bd2.Core.PerformanceAggregate;
using bd2.Core.StagingAggregate;
using bd2.Infrastructure.DTO;

namespace bd2.Application.Abstractions;

public interface IPerformanceRepository : IGenericRepository<Performance>
{
    public void UpdateArtistInPerformance(int performanceId, int roleId, int artistId);
    public void MovePerformanceToHall(int performanceId, int newHallId);
    public void ReturnTicket(int performanceId);
    public void BuyTicket(int performanceId);
    public IEnumerable<Role> GetRoles(int[] ids); 
    public IEnumerable<PerformanceFilterWrapper> FilterPerformances(PerformanceFilter filter);
}