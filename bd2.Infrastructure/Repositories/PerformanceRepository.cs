using System.Text.Json;
using bd2.Application.Abstractions;
using bd2.Application.DTO;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;
using bd2.Core.PerformanceAggregate;
using bd2.Core.StagingAggregate;

namespace bd2.Infrastructure.Repositories;

public class PerformanceRepository(
    GenericRepository<PerformanceDto> performanceRepository,
    IStagingRepository stagingRepository,
    IHallRepository hallRepository,
    GenericRepository<ArtistsInPerformanceDto> artistsInPerformanceRepository,
    GenericRepository<RoleDto> roleRepository) : IPerformanceRepository
{
    private GenericRepository<PerformanceDto> _performanceRepository = performanceRepository;
    private IStagingRepository _stagingRepository = stagingRepository;
    private IHallRepository _hallRepository = hallRepository;
    private GenericRepository<ArtistsInPerformanceDto> _artistsInPerformanceRepository = artistsInPerformanceRepository;
    private GenericRepository<RoleDto> _roleRepository = roleRepository;
    
    public Performance? GetById(int id)
    {
        var performanceDto = _performanceRepository.GetById(id);
        if (performanceDto == null)
            return null;

        return MapPerformanceDtoToPerformance(performanceDto);
    }
    
    public IEnumerable<Performance> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];
        
        ids = ids.Distinct().ToArray();
            
        var performanceDtos = _performanceRepository.GetByIds(ids.Select(x => x).ToArray()).ToList();
        if(performanceDtos.Count < ids.Length) throw new EntityNotFoundException(nameof(PerformanceDto));
        return MapPerformanceDtosToPerformances(performanceDtos);
    }

    public IEnumerable<Performance> GetAll()
    {
        var performanceDtos = _performanceRepository.GetAll();
        return MapPerformanceDtosToPerformances(performanceDtos);
    }

    public int Create(Performance entity)
    {
        var artistsData = entity.Artists.Select(a => new { artist_id = a.ArtistId, role_id = a.RoleId });
        var artistsJson = JsonSerializer.Serialize(artistsData);

        var parameters = new Dictionary<string, object>
        {
            { "@p_start_date_time", entity.StartDate },
            { "@p_staging_id", entity.Staging.Id },
            { "@p_hall_id", entity.Hall.HallId },
            { "@p_artists_data", artistsJson }
        };

        return _performanceRepository.ExecuteScalar<int>("SELECT add_performance(@p_start_date_time, @p_staging_id, @p_hall_id, @p_artists_data::jsonb)", parameters);
    }

    public void MovePerformanceToHall(int performanceId, int newHallId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_performance_id", performanceId },
            { "@p_new_hall_id", newHallId }
        };

        _performanceRepository.ExecuteCommand("SELECT move_performance_to_hall(@p_performance_id, @p_new_hall_id)", parameters);
    }

    public void UpdateArtistInPerformance(int performanceId, int roleId, int artistId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_performance_id", performanceId },
            { "@p_role_id", roleId },
            { "@p_artist_id", artistId }
        };

        _performanceRepository.ExecuteCommand("SELECT update_artist_in_performance(@p_performance_id, @p_role_id, @p_artist_id)", parameters);
    }

    public void Update(Performance entity) {
        _artistsInPerformanceRepository.ExecuteCommand($"DELETE FROM ArtistsInPerformances WHERE PerformanceId = {entity.Id}");

        foreach(var artist in entity.Artists){
            _artistsInPerformanceRepository.Create(new ArtistsInPerformanceDto{
                PerformanceId = entity.Id,
                ArtistId = artist.ArtistId,
                RoleId = artist.RoleId
            });
        }

        var performanceDto = new PerformanceDto
        {
            Id = entity.Id,
            StartDateTime = entity.StartDate,
            StagingId = entity.Staging.Id,
            HallId = entity.Hall.HallId,
            SoldTicketsCount = entity.SoldTicketsCount
        };
        _performanceRepository.Update(performanceDto);
    }
    
    public IEnumerable<PerformanceFilterWrapper> FilterPerformances(PerformanceFilter filter)
    {
        var parameters = new Dictionary<string, object>();
        string query = "SELECT * FROM AllPerformancesData WHERE 1=1";

        if (filter.StartDateTimeFrom.HasValue)
        {
            query += " AND StartDateTime >= @StartDateTimeFrom";
            parameters.Add("@StartDateTimeFrom", filter.StartDateTimeFrom.Value);
        }

        if (filter.StartDateTimeTo.HasValue)
        {
            query += " AND StartDateTime < @StartDateTimeTo";
            parameters.Add("@StartDateTimeTo", filter.StartDateTimeTo.Value);
        }

        if (!string.IsNullOrEmpty(filter.OriginName))
        {
            query += " AND OriginName = @OriginName";
            parameters.Add("@OriginName", filter.OriginName);
        }

        if (!string.IsNullOrEmpty(filter.ComposerName))
        {
            query += " AND ComposerName = @ComposerName";
            parameters.Add("@ComposerName", filter.ComposerName);
        }

        if (!string.IsNullOrEmpty(filter.WriterName))
        {
            query += " AND WriterName = @WriterName";
            parameters.Add("@WriterName", filter.WriterName);
        }

        if (!string.IsNullOrEmpty(filter.StagingComposerName))
        {
            query += " AND StagingComposerName = @StagingComposerName";
            parameters.Add("@StagingComposerName", filter.StagingComposerName);
        }

        if (!string.IsNullOrEmpty(filter.DirectorName))
        {
            query += " AND DirectorName = @DirectorName";
            parameters.Add("@DirectorName", filter.DirectorName);
        }

        if (!string.IsNullOrEmpty(filter.ArtistName))
        {
            query += " AND ArtistName = @ArtistName";
            parameters.Add("@ArtistName", filter.ArtistName);
        }

        return _performanceRepository.ExecuteQuery<PerformanceFilterWrapper>(query, parameters);
    }

    public IEnumerable<Role> GetRoles(int[] ids)
    {
        return _roleRepository.GetByIds(ids).Select(x => new Role(x.Id, x.RoleName));
    }
    
    
    public void BuyTicket(int performanceId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_performance_id", performanceId }
        };

        _performanceRepository.ExecuteCommand("CALL buy_ticket(@p_performance_id)", parameters);
    }

    public void ReturnTicket(int performanceId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@performance_id", performanceId }
        };

        _performanceRepository.ExecuteCommand("CALL return_ticket(@performance_id)", parameters);
    }


    public void Delete(int id)
    {
        _performanceRepository.Delete(id);
    }

    private Performance MapPerformanceDtoToPerformance(PerformanceDto performanceDto)
    {
        var staging = _stagingRepository.GetById(performanceDto.StagingId);
        var hall = _hallRepository.GetById(performanceDto.HallId);

        if (staging == null || hall == null)
            throw new EntityNotFoundException(nameof(Staging));

        var artistsInPerformanceDtos = _artistsInPerformanceRepository.ExecuteQuery<ArtistsInPerformanceDto>("SELECT * FROM ArtistsInPerformances WHERE PerformanceId = @Id", new Dictionary<string, object> { { "@Id", performanceDto.Id } });
        var artists = artistsInPerformanceDtos.Select(dto => new Performance.ArtistsInPerformance(dto.ArtistId, dto.RoleId)).ToList();

        return new Performance(performanceDto.Id, performanceDto.StartDateTime, staging, hall, performanceDto.SoldTicketsCount, artists);
    }
    
    private IEnumerable<Performance> MapPerformanceDtosToPerformances(IEnumerable<PerformanceDto> performanceDtos)
    {
        performanceDtos = performanceDtos.ToList();
        if (!performanceDtos.Any()) return [];
        var stagingIds = performanceDtos.Select(dto => dto.StagingId).ToList();
        var hallIds = performanceDtos.Select(dto => dto.HallId).ToList();
        var performanceIds = performanceDtos.Select(dto => dto.Id).ToList();

        var stagings = _stagingRepository.GetByIds(stagingIds.ToArray()).ToDictionary(s => s.Id);
        var halls = _hallRepository.GetByIds(hallIds.ToArray()).ToDictionary(h => h.HallId);

        var artistsInPerformanceDtos = _artistsInPerformanceRepository.ExecuteQuery<ArtistsInPerformanceDto>($"SELECT * FROM ArtistsInPerformances WHERE PerformanceId IN ({string.Join(",", performanceIds)})");
        var artistsByPerformanceId = artistsInPerformanceDtos.GroupBy(dto => dto.PerformanceId)
            .ToDictionary(group => group.Key, group => group.Select(a => new Performance.ArtistsInPerformance(a.ArtistId, a.RoleId)).ToList());

        return performanceDtos.Select(dto =>
        {
            var staging = stagings.GetValueOrDefault(dto.StagingId);
            var hall = halls.GetValueOrDefault(dto.HallId);

            if (staging == null || hall == null)
                throw new EntityNotFoundException(nameof(Staging));

            var artists = artistsByPerformanceId.GetValueOrDefault(dto.Id, new List<Performance.ArtistsInPerformance>());

            return new Performance(dto.Id, dto.StartDateTime, staging, hall, dto.SoldTicketsCount, artists);
        });
    }
}