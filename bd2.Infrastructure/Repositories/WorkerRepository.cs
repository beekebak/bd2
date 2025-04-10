using bd2.Application.Abstractions;
using bd2.Core.Exceptions;
using bd2.Core.Worker;
using bd2.Infrastructure.DTO;

namespace bd2.Infrastructure.Repositories;

public class WorkerRepository : IWorkerRepository
{
    private GenericRepository<WorkerDto> _workerRepository;
    private GenericRepository<ArtistDto> _artistRepository;
    
    public WorkerRepository(GenericRepository<WorkerDto> workerRepository, GenericRepository<ArtistDto> artistRepository)
    {
        _workerRepository = workerRepository;
        _artistRepository = artistRepository;
    }
    
    public Worker GetById(int id)
    {
        var workerDto = _workerRepository.GetById(id);
        if (workerDto!.Specialty == "Актер")
        {
            var artistDto = _artistRepository.GetById(id);
            return new Artist(workerDto.Id, workerDto.Name, workerDto.Specialty, artistDto!.Grade);
        }
        return new Worker(workerDto.Id, workerDto.Name, workerDto.Specialty);
    }

    public IEnumerable<Worker> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];
        
        ids = ids.Distinct().ToArray();
        
        var dtos = _workerRepository.GetByIds(ids).ToList();
        if(ids != null && dtos.Count < ids.Length) throw new EntityNotFoundException(nameof(WorkerDto));
        var artistsIds = dtos.Where(x => x.Specialty == "Актер").Select(x => x.Id);
        var artistDtos = _artistRepository.GetByIds(artistsIds.ToArray()).ToList();
        var result = dtos.Where(dto => dto.Specialty != "Актер")
            .Select(dto => new Worker(dto.Id, dto.Name, dto.Specialty));
        result = result.Union(dtos.Where(x => x.Specialty == "Актер").
            Select(dto => new Artist(dto.Id, dto.Name, dto.Specialty, artistDtos.Find(x => x.Id == dto.Id)!.Grade)));
        return result;
    }

    public IEnumerable<Worker> GetAll()
    {
        var dtos = _workerRepository.GetAll().ToList();
        var artistsIds = dtos.Where(x => x.Specialty == "Актер").Select(x => x.Id);
        var artistDtos = _artistRepository.GetByIds(artistsIds.ToArray()).ToList();
        var result = dtos.Where(dto => dto.Specialty != "Актер")
            .Select(dto => new Worker(dto.Id, dto.Name, dto.Specialty));
        result = result.Union(dtos.Where(x => x.Specialty == "Актер").
            Select(dto => new Artist(dto.Id, dto.Name, dto.Specialty, artistDtos.Find(x => x.Id == dto.Id)!.Grade)));
        return result;
    }

    public int Create(Worker entity)
    {
        int res;
        if (entity is Artist artist)
        {
            string query = "SELECT * FROM add_artist(@Name, @Grade)";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", artist.Name },
                { "@Grade", artist.Grade }
            };

            res = _workerRepository.ExecuteScalar<int>(query, parameters);
        }
        else
        {
            string query = "SELECT * FROM add_worker(@Name, @Specialty)";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", entity.Name },
                { "@Specialty", entity.Specialty }
            };

            res = _workerRepository.ExecuteScalar<int>(query, parameters);
        }
        return res;
    }

    public void Update(Worker entity)
    {
        var dto = new WorkerDto()
        {
            Id = entity.Id,
            Name = entity.Name,
            Specialty = entity.Specialty
        };
        _workerRepository.Update(dto);
        if (entity is Artist artist)
        {
            var artistDto = new ArtistDto()
            {
                Id = artist.Id,
                Grade = artist.Grade
            };
            _artistRepository.Update(artistDto);
        }
    }

    public void Delete(int id)
    {
        _workerRepository.Delete(id);
    }
}