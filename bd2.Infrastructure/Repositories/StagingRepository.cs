using System.Text.Json;
using bd2.Application.Abstractions;
using bd2.Core;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;

using bd2.Core.StagingAggregate;
using bd2.Core.Worker;

namespace bd2.Infrastructure.Repositories;

public class StagingRepository(
    GenericRepository<StagingDto> stagingRepository,
    IWorkerRepository workerRepository,
    IOriginRepository originRepository,
    IInventoryRepository inventoryRepository,
    GenericRepository<RoleDto> roleRepository,
    GenericRepository<NeededInventoryDto> neededInventoryRepository) : IStagingRepository
{
    private GenericRepository<StagingDto> _stagingRepository = stagingRepository;
    private IWorkerRepository _workerRepository = workerRepository;
    private IOriginRepository _originRepository = originRepository;
    private IInventoryRepository _inventoryRepository = inventoryRepository;
    private GenericRepository<RoleDto> _roleRepository = roleRepository;
    private GenericRepository<NeededInventoryDto> _neededInventoryRepository = neededInventoryRepository;

    public Staging? GetById(int id)
    {
        var stagingDto = _stagingRepository.GetById(id);
        if (stagingDto == null)
            return null;

        return MapStagingDtoToStaging(stagingDto);
    }
    
    public IEnumerable<Staging> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];
            
        var stagingDtos = _stagingRepository.GetByIds(ids).ToList();
        if(stagingDtos.Count < ids.Length) throw new EntityNotFoundException(nameof(StagingDto));
        return MapStagingDtosToStagings(stagingDtos);
    }

    public IEnumerable<Staging> GetAll()
    {
        var stagingDtos = _stagingRepository.GetAll();
        return MapStagingDtosToStagings(stagingDtos);
    }

    public void Create(Staging entity)
    {
        var inventoryJson = JsonSerializer.Serialize(entity.Inventory.Select(i => new { inventory_id = i.Key.InventoryId, count = i.Value }));
        var rolesJson = JsonSerializer.Serialize(entity.Roles.Select(r => new { role_name = r.Name }));

        var parameters = new Dictionary<string, object>
        {
            { "@p_duration", entity.Duration.ToTimeSpan() },
            { "@p_director_id", entity.Director.Id },
            { "@p_staging_composer_id", entity.StagingComposer.Id },
            { "@p_origin_id", entity.OriginId },
            { "@p_inventory", inventoryJson },
            { "@p_roles", rolesJson }
        };
        
        var stagingId = _stagingRepository.ExecuteScalar<int>("SELECT add_staging_with_inventory_roles(@p_duration, @p_director_id, @p_staging_composer_id, @p_origin_id, @p_inventory, @p_roles)", parameters);
        if (stagingId == 0)
        {
            throw new Exception("Не удалось создать постановку.");
        }
    }

    public void Update(Staging entity)
    {
        var inventoryJson = JsonSerializer.Serialize(entity.Inventory.Select(i => new { inventory_id = i.Key.InventoryId, count = i.Value }));
        var rolesJson = JsonSerializer.Serialize(entity.Roles.Select(r => new { role_name = r.Name }));

        var parameters = new Dictionary<string, object>
        {
            { "@p_staging_id", entity.Id },
            { "@p_duration", entity.Duration.ToTimeSpan() },
            { "@p_director_id", entity.Director.Id },
            { "@p_staging_composer_id", entity.StagingComposer.Id },
            { "@p_origin_id", entity.OriginId },
            { "@p_inventory", inventoryJson },
            { "@p_roles", rolesJson }
        };
        _stagingRepository.ExecuteCommand("SELECT update_staging_with_inventory_roles(@p_staging_id, @p_duration, @p_director_id, @p_staging_composer_id, @p_origin_id, @p_inventory, @p_roles)", parameters);
    }

    public void Delete(int id)
    {
        _stagingRepository.Delete(id);
    }

    private Staging MapStagingDtoToStaging(StagingDto stagingDto)
    {
        var director = _workerRepository.GetById(stagingDto.DirectorId);
        var stagingComposer = _workerRepository.GetById(stagingDto.StagingComposerId);
        var origin = _originRepository.GetById(stagingDto.OriginId);

        if (director == null || stagingComposer == null || origin == null)
            throw new EntityNotFoundException(nameof(Worker));

        var rolesDtos = _roleRepository.ExecuteQuery<RoleDto>("SELECT * FROM Roles WHERE StagingId = @Id", new Dictionary<string, object> { { "@Id", stagingDto.StagingId } });
        var roles = rolesDtos.Select(dto => new Role(dto.RoleName)).ToList();

        var neededInventoryDtos = _neededInventoryRepository.ExecuteQuery<NeededInventoryDto>("SELECT * FROM NeededInventory WHERE StagingId = @Id", new Dictionary<string, object> { { "@Id", stagingDto.StagingId } });
        var inventory = new Dictionary<Inventory, int>();

        foreach (var neededInventoryDto in neededInventoryDtos)
        {
            var inv = _inventoryRepository.GetById(neededInventoryDto.InventoryId);
            if (inv != null)
            {
                inventory[inv] = neededInventoryDto.Count;
            }
        }

        return new Staging(stagingDto.StagingId, TimeOnly.FromTimeSpan(stagingDto.Duration), director, stagingComposer, stagingDto.OriginId, roles, inventory);
    }
    
    private IEnumerable<Staging> MapStagingDtosToStagings(IEnumerable<StagingDto> stagingDtos)
    {
        stagingDtos = stagingDtos.ToList();
        var directorIds = stagingDtos.Select(dto => dto.DirectorId).ToList();
        var stagingComposerIds = stagingDtos.Select(dto => dto.StagingComposerId).ToList();
        var originIds = stagingDtos.Select(dto => dto.OriginId).ToList();
        var stagingIds = stagingDtos.Select(dto => dto.StagingId).ToList();

        var directors = _workerRepository.GetByIds(directorIds.ToArray()).ToDictionary(w => w.Id);
        var stagingComposers = _workerRepository.GetByIds(stagingComposerIds.ToArray()).ToDictionary(w => w.Id);
        var origins = _originRepository.GetByIds(originIds.ToArray()).ToDictionary(o => o.OriginId);

        var rolesDtos = _roleRepository.ExecuteQuery<RoleDto>($"SELECT * FROM Roles WHERE StagingId IN ({string.Join(",", stagingIds)})");
        var rolesByStagingId = rolesDtos.GroupBy(dto => dto.StagingId)
            .ToDictionary(group => group.Key, group => group.Select(s => new Role(s.RoleName)).ToList());

        var neededInventoryDtos = _neededInventoryRepository.ExecuteQuery<NeededInventoryDto>($"SELECT * FROM NeededInventory WHERE StagingId IN ({string.Join(",", stagingIds)})");
        var inventoryByStagingId = neededInventoryDtos.GroupBy(dto => dto.StagingId)
            .ToDictionary(group => group.Key, group => group.ToDictionary(ni => _inventoryRepository.GetById(ni.InventoryId), ni => ni.Count));

        return stagingDtos.Select(dto =>
        {
            var director = directors.GetValueOrDefault(dto.DirectorId);
            var stagingComposer = stagingComposers.GetValueOrDefault(dto.StagingComposerId);
            var origin = origins.GetValueOrDefault(dto.OriginId);

            if (director == null || stagingComposer == null || origin == null)
                throw new EntityNotFoundException(nameof(Worker));

            var roles = rolesByStagingId.GetValueOrDefault(dto.StagingId, new List<Role>());
            var inventory = inventoryByStagingId!.GetValueOrDefault(dto.StagingId, new Dictionary<Inventory, int>());

            return new Staging(dto.StagingId, TimeOnly.FromTimeSpan(dto.Duration), director, stagingComposer, dto.OriginId, roles, inventory);
        });
    }
}