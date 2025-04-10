using bd2.Application.DTO;
using bd2.Core.StagingAggregate;

namespace bd2.Application.Abstractions;

public interface IStagingRepository : IGenericRepository<Staging>
{
    public IEnumerable<FilteredStaging> FilterStagings(StagingFilter filter);
}