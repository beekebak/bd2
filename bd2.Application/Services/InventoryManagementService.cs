using bd2.Application.Abstractions;
using bd2.Core;

namespace bd2.Application.Services;

public class InventoryManagementService(IInventoryRepository inventoryRepository)
{
    public void AddInventory(Inventory inventory)
    {
        inventoryRepository.Create(inventory);
    }

    public Inventory GetInventory(int inventoryId)
    {
        return inventoryRepository.GetById(inventoryId)!;
    }

    public IEnumerable<Inventory> GetInventory()
    {
        return inventoryRepository.GetAll();
    }

    public IEnumerable<Inventory> GetInventory(int[] ids)
    {
        return inventoryRepository.GetByIds(ids);
    }
    
    public void UpdateInventory(Inventory inventory)
    {
        inventoryRepository.Update(inventory);
    }

    public void DeleteInventory(int inventoryId)
    {
        inventoryRepository.Delete(inventoryId);
    }
}