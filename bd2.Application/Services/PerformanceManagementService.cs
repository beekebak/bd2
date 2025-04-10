using bd2.Application.Abstractions;
using bd2.Application.DTO;
using bd2.Core.PerformanceAggregate;
using bd2.Core.StagingAggregate;
using bd2.Core.Worker;
using bd2.Infrastructure.DTO;

namespace bd2.Application.Services;

public class PerformanceManagementService(
    IPerformanceRepository performanceRepository,
    IHallRepository hallRepository,
    StagingManagementService stagingService,
    WorkerManagementService workerService)
{
    public void AddPerformance(Performance performance)
    {
        performanceRepository.Create(performance);
    }

    public Performance GetPerformance(int performanceId)
    {
        return performanceRepository.GetById(performanceId)!;
    }

    public IEnumerable<Performance> GetPerformances()
    {
        return performanceRepository.GetAll();
    }
    
    public IEnumerable<FilteredPerformance> GetPerformances(PerformanceFilter filter)
    {
        return performanceRepository.FilterPerformances(filter)
            .GroupBy(w => w.Id)
            .Select(group => new FilteredPerformance
            {
                Id = group.Key,
                StartDateTime = group.First().StartDateTime,
                OriginName = group.First().OriginName,
                ComposerName = group.First().ComposerName,
                WriterName = group.First().WriterName,
                StagingComposerName = group.First().StagingComposerName,
                DirectorName = group.First().DirectorName,
                ArtistNames = group
                    .Where(w => !string.IsNullOrEmpty(w.ArtistName))
                    .Select(w => w.ArtistName!)
                    .Distinct()
                    .ToList()
            });
    }

    public IEnumerable<Performance> GetPerformances(int[] ids)
    {
        return performanceRepository.GetByIds(ids);
    }
    
    public void UpdatePerformance(Performance performance)
    {
        performanceRepository.Update(performance);
    }
    
    public void DeletePerformance(int performanceId)
    {
        performanceRepository.Delete(performanceId);
    }
    
    public void AddHall(Hall hall)
    {
        hallRepository.Create(hall);
    }

    public Hall GetHall(int hallId)
    {
        return hallRepository.GetById(hallId)!;
    }

    public IEnumerable<Hall> GetHalls()
    {
        return hallRepository.GetAll();
    }

    public IEnumerable<Hall> GetHalls(int[] ids)
    {
        return hallRepository.GetByIds(ids);
    }
    
    public void UpdateHall(Hall hall)
    {
        hallRepository.Update(hall);
    }
    
    public void DeleteHall(int hallId)
    {
        hallRepository.Delete(hallId);
    }
    
    public List<Staging> GetStagings()
    {
        return stagingService.GetStagings().ToList();
    }

    public List<Artist> GetArtists()
    {
        return workerService.GetWorkers().OfType<Artist>().ToList();
    }
    
    public List<Artist> GetArtists(int[] ids)
    {
        return workerService.GetWorkers(ids).OfType<Artist>().ToList();
    }

    public List<Role> GetRoles(int[] ids)
    {
        return performanceRepository.GetRoles(ids).ToList();
    }

    public Staging GetStagingById(int stagingId)
    {
        return stagingService.GetStaging(stagingId);
    }

    public void BuyTicket(int performanceId)
    {
        performanceRepository.BuyTicket(performanceId);
    }

    public void ReturnTicket(int performanceId)
    {
        performanceRepository.ReturnTicket(performanceId);
    }

    public void UpdateArtistRole(int performanceId, int roleId, int artistId)
    {
        performanceRepository.UpdateArtistInPerformance(performanceId, roleId, artistId);
    }

    public void MoveToOtherHall(int performanceId, int hallId)
    {
        performanceRepository.MovePerformanceToHall(performanceId, hallId);
    }
}