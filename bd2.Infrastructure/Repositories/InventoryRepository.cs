using bd2.Application.Abstractions;
using bd2.Core;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;

namespace bd2.Infrastructure.Repositories;

public class InventoryRepository(GenericRepository<InventoryDto> repository) : IInventoryRepository
{
    private GenericRepository<InventoryDto> _repository = repository;

    public Inventory GetById(int id)
    {
        var inventory = _repository.GetById(id);
        if(inventory == null) throw new EntityNotFoundException(nameof(InventoryDto));
        return new Inventory(inventory.InventoryName, inventory.Id, inventory.TotalAmount);
    }

    public IEnumerable<Inventory> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];
        
        ids = ids.Distinct().ToArray();
        var dtos = _repository.GetByIds(ids).ToList();
        if(ids != null && dtos.Count < ids.Length) throw new EntityNotFoundException(nameof(HallDto));
        return dtos.Select(dto => new Inventory(dto.InventoryName, dto.Id, dto.TotalAmount));
    }

    public IEnumerable<Inventory> GetAll()
    {
        var dtos = _repository.GetAll();
        return dtos.Select(dto => new Inventory(dto.InventoryName, dto.Id, dto.TotalAmount));
    }

    public int Create(Inventory entity)
    {
        var dto = new InventoryDto
        { 
            InventoryName = entity.Name,
            Id = entity.InventoryId,
            TotalAmount = entity.TotalAmount
        };
        return _repository.Create(dto);
    }

    public void Update(Inventory entity)
    {
        var dto = new InventoryDto
        { 
            InventoryName = entity.Name,
            Id = entity.InventoryId,
            TotalAmount = entity.TotalAmount
        };
        _repository.Update(dto);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }

    public IEnumerable<Inventory> GetAvailableInventory(DateTime checkTime)
    {
        string query = "SELECT * FROM get_available_inventory(@CheckTime)";

        var parameters = new Dictionary<string, object>
        {
            { "@CheckTime", checkTime }
        };

        var dtos = _repository.ExecuteQuery<InventoryDto>(query, parameters);
        return dtos.Select(dto => new Inventory(dto.InventoryName, dto.Id, dto.TotalAmount));
    }
}