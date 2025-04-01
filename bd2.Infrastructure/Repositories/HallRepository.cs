using bd2.Application.Abstractions;
using bd2.Core.Exceptions;
using bd2.Core.PerformanceAggregate;
using bd2.Infrastructure.DTO;

namespace bd2.Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private readonly GenericRepository<HallDto> _repository;

    public HallRepository(GenericRepository<HallDto> repository)
    {
        _repository = repository;
    }
    
    public Hall GetById(int id)
    {
        var dto = _repository.GetById(id);
        if(dto == null) throw new EntityNotFoundException(nameof(HallDto));
        return new Hall(dto.HallId, dto.Capacity);
    }

    public IEnumerable<Hall> GetByIds(int[]? ids)
    {
        var dtos = _repository.GetByIds(ids).ToList();
        if(ids != null && dtos.Count < ids.Length) throw new EntityNotFoundException(nameof(HallDto));
        return dtos.Select(dto => new Hall(dto.HallId, dto.Capacity));
    }

    public IEnumerable<Hall> GetAll()
    {
        var dtos = _repository.GetAll();
        return dtos.Select(dto => new Hall(dto.HallId, dto.Capacity));
    }

    public void Create(Hall entity)
    {
        var dto = new HallDto
        {
            Capacity = entity.Capacity,
            HallId = entity.HallId
        };
        _repository.Create(dto);
    }

    public void Update(Hall entity)
    {
        var dto = new HallDto
        {
            Capacity = entity.Capacity,
            HallId = entity.HallId
        };
        _repository.Update(dto);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }
}