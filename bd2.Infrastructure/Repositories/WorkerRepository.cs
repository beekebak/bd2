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
        if (workerDto!.Specialty == "Artist")
        {
            var artistDto = _artistRepository.GetById(id);
            return new Artist(workerDto.WorkerId, workerDto.Name, workerDto.Specialty, artistDto!.Grade);
        }
        return new Worker(workerDto.WorkerId, workerDto.Name, workerDto.Specialty);
    }

    public IEnumerable<Worker> GetByIds(int[]? ids)
    {
        var dtos = _workerRepository.GetByIds(ids).ToList();
        if(ids != null && dtos.Count < ids.Length) throw new EntityNotFoundException(nameof(HallDto));
        var artistsIds = dtos.Where(x => x.Specialty == "Artist").Select(x => x.WorkerId);
        var artistDtos = _artistRepository.GetByIds(artistsIds.ToArray()).ToList();
        var result = dtos.Where(dto => dto.Specialty != "Artist")
            .Select(dto => new Worker(dto.WorkerId, dto.Name, dto.Specialty));
        result = result.Union(dtos.Where(x => x.Specialty == "Artist").
            Select(dto => new Artist(dto.WorkerId, dto.Name, dto.Specialty, artistDtos.Find(x => x.ArtistId == dto.WorkerId)!.Grade)));
        return result;
    }

    public IEnumerable<Worker> GetAll()
    {
        var dtos = _workerRepository.GetAll().ToList();
        var artistsIds = dtos.Where(x => x.Specialty == "Artist").Select(x => x.WorkerId);
        var artistDtos = _artistRepository.GetByIds(artistsIds.ToArray()).ToList();
        var result = dtos.Where(dto => dto.Specialty != "Artist")
            .Select(dto => new Worker(dto.WorkerId, dto.Name, dto.Specialty));
        result = result.Union(dtos.Where(x => x.Specialty == "Artist").
            Select(dto => new Artist(dto.WorkerId, dto.Name, dto.Specialty, artistDtos.Find(x => x.ArtistId == dto.WorkerId)!.Grade)));
        return result;
    }

    public void Create(Worker entity)
    {
        if (entity is Artist artist)
        {
            string query = "SELECT * FROM add_artist(@Name, @Grade)";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", artist.Name },
                { "@Grade", artist.Grade }
            };

            _workerRepository.ExecuteCommand(query, parameters);
        }
        else
        {
            string query = "SELECT * FROM add_worker(@Name, @Specialty)";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", entity.Name },
                { "@Specialty", entity.Specialty }
            };

            _workerRepository.ExecuteCommand(query, parameters);
        }
    }

    public void Update(Worker entity)
    {
        var dto = new WorkerDto()
        {
            WorkerId = entity.Id,
            Name = entity.Name,
            Specialty = entity.Specialty
        };
        _workerRepository.Update(dto);
        if (entity is Artist artist)
        {
            var artistDto = new ArtistDto()
            {
                ArtistId = artist.Id,
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