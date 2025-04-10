using bd2.Application.Abstractions;
using bd2.Application.DTO;
using bd2.Core;
using bd2.Core.StagingAggregate;
using bd2.Core.Worker;

namespace bd2.Application.Services;

public class StagingManagementService(
    IStagingRepository stagingRepository,
    OriginsManagementService originsManagementService,
    WorkerManagementService workerManagementService,
    InventoryManagementService inventoryManagementService
    )
{
    public void AddStaging(Staging staging)
    {
        stagingRepository.Create(staging);
    }

    public Staging GetStaging(int stagingId)
    {
        return stagingRepository.GetById(stagingId)!;
    }

    public IEnumerable<Staging> GetStagings()
    {
        return stagingRepository.GetAll();
    }

    public IEnumerable<Staging> GetStagings(int[] ids)
    {
        return stagingRepository.GetByIds(ids);
    }
    
    public void UpdateStaging(Staging staging)
    {
        stagingRepository.Update(staging);
    }

    public void DeleteStaging(int stagingId)
    {
        stagingRepository.Delete(stagingId);
    }

    public List<Worker> GetAllWorkers()
    {
        return workerManagementService.GetWorkers().ToList();
    }
    
    public List<Origin> GetAllOrigins()
    {
        return originsManagementService.GetOrigins().ToList();
    }
    
    public List<Inventory> GetAllInventory()
    {
        return inventoryManagementService.GetInventory().ToList();
    }

    public List<FilteredStaging> GetStagings(StagingFilter filter)
    {
        return stagingRepository.FilterStagings(filter).ToList();
    }
}