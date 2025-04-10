using bd2.Application.Abstractions;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;
using bd2.Core.StagingAggregate;

namespace bd2.Infrastructure.Repositories;

public class OriginRepository(
    GenericRepository<OriginDto> originRepository,
    IAuthorRepository authorRepository) : IOriginRepository
{
    private GenericRepository<OriginDto> _originRepository = originRepository;
    private IGenericRepository<Author> _authorRepository = authorRepository;

    public Origin GetById(int id)
    {
        var originDto = _originRepository.GetById(id);
        if (originDto == null)
            throw new EntityNotFoundException(nameof(OriginDto));

        return MapOriginDtoToOrigin(originDto);
    }
    
    public IEnumerable<Origin> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];
        
        ids = ids.Distinct().ToArray();
            
        var originDtos = _originRepository.GetByIds(ids).ToList();
        if(originDtos.Count < ids.Length) throw new EntityNotFoundException(nameof(OriginDto));
        return MapOriginDtosToOrigins(originDtos);
    }

    public IEnumerable<Origin> GetAll()
    {
        var originDtos = _originRepository.GetAll();
        return MapOriginDtosToOrigins(originDtos);
    }

    public int Create(Origin entity)
    {
        var originDto = new OriginDto
        {
            OriginName = entity.OriginName,
            OriginComposerId = entity.OriginComposer.Id,
            WriterId = entity.Writer.Id
        };
        return _originRepository.Create(originDto);
    }

    public void Update(Origin entity)
    {
        var originDto = new OriginDto
        {
            Id = entity.OriginId,
            OriginName = entity.OriginName,
            OriginComposerId = entity.OriginComposer.Id,
            WriterId = entity.Writer.Id
        };
        _originRepository.Update(originDto);
    }

    public void Delete(int id)
    {
        _originRepository.Delete(id);
    }
    
    private Origin MapOriginDtoToOrigin(OriginDto originDto)
    {
        var composer = _authorRepository.GetById(originDto.OriginComposerId);
        var writer = _authorRepository.GetById(originDto.WriterId);

        if (composer == null || writer == null)
            throw new EntityNotFoundException(nameof(AuthorDto));

        return new Origin(originDto.Id, originDto.OriginName, writer, composer);
    }
    
    private IEnumerable<Origin> MapOriginDtosToOrigins(IEnumerable<OriginDto> originDtos)
    {
        originDtos = originDtos.ToList();
        if (!originDtos.Any()) return [];
        var composerIds = originDtos.Select(dto => dto.OriginComposerId).ToList();
        var writerIds = originDtos.Select(dto => dto.WriterId).ToList();
        var authorIds = composerIds.Union(writerIds).ToList();

        var authors = _authorRepository.GetByIds(authorIds.ToArray()).ToDictionary(a => a.Id);

        return originDtos.Select(dto =>
        {
            var composer = authors.GetValueOrDefault(dto.OriginComposerId);
            var writer = authors.GetValueOrDefault(dto.WriterId);

            if (composer == null || writer == null)
                throw new EntityNotFoundException(nameof(AuthorDto));

            return new Origin(dto.Id, dto.OriginName, writer, composer);
        });
    }
}