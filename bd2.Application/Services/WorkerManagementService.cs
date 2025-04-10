using bd2.Application.Abstractions;
using bd2.Core.Worker;

namespace bd2.Application.Services;

public class WorkerManagementService(IWorkerRepository workerRepository)
{
    public void AddWorker(Worker worker)
    {
        workerRepository.Create(worker);
    }

    public Worker GetWorker(int workerId)
    {
        return workerRepository.GetById(workerId)!;
    }

    public IEnumerable<Worker> GetWorkers()
    {
        return workerRepository.GetAll();
    }

    public IEnumerable<Worker> GetWorkers(int[] ids)
    {
        return workerRepository.GetByIds(ids);
    }
    
    public void UpdateWorker(Worker worker)
    {
        workerRepository.Update(worker);
    }

    public void DeleteWorker(int workerId)
    {
        workerRepository.Delete(workerId);
    }
}