using bd2.Core;

namespace bd2.Application.Abstractions;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    IEnumerable<Inventory> GetAvailableInventory(DateTime checkTime);
}