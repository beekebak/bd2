using bd2.Application.Abstractions;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;
using bd2.Core.StagingAggregate;

namespace bd2.Infrastructure.Repositories;

public class AuthorRepository(GenericRepository<AuthorDto> repository, GenericRepository<AuthorsSpecialtyDto> specialtyRepository) : IGenericRepository<Author>
{
    private GenericRepository<AuthorDto> _authorRepository = repository;
    private GenericRepository<AuthorsSpecialtyDto> _specialtyRepository = specialtyRepository;

    public Author? GetById(int id)
    {
        var authorDto = _authorRepository.GetById(id);
        if (authorDto == null)
            return null;

        return MapAuthorDtoToAuthor(authorDto);
    }
    
    public IEnumerable<Author> GetByIds(int[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];
            
        var authorDtos = _authorRepository.GetByIds(ids).ToList();
        if(authorDtos.Count < ids.Length) throw new EntityNotFoundException(nameof(AuthorDto));
        return MapAuthorDtosToAuthors(authorDtos);
    }

    public IEnumerable<Author> GetAll()
    {
        var authorDtos = _authorRepository.GetAll();
        return MapAuthorDtosToAuthors(authorDtos);
    }

    public void Create(Author entity)
    {
        var authorDto = new AuthorDto
        {
            AuthorName = entity.AuthorName
        };
        _authorRepository.Create(authorDto);
    }

    public void Update(Author entity)
    {
        var authorDto = new AuthorDto
        {
            AuthorId = entity.id,
            AuthorName = entity.AuthorName
        };
        _authorRepository.Update(authorDto);
    }

    public void Delete(int id)
    {
        _authorRepository.Delete(id);
    }

    private Author MapAuthorDtoToAuthor(AuthorDto authorDto)
    {
        var specialtiesDto = _specialtyRepository.ExecuteQuery<AuthorsSpecialtyDto>(
            "SELECT * FROM AuthorsSpecialties WHERE AuthorId = @Id",
            new Dictionary<string, object> { { "@Id", authorDto.AuthorId } }
        );

        var specialties = specialtiesDto.Select(s => new AuthorSpecialty(s.SpecialtyName)).ToList();

        return new Author(authorDto.AuthorId, authorDto.AuthorName, specialties);
    }
    
    private IEnumerable<Author> MapAuthorDtosToAuthors(IEnumerable<AuthorDto> authorDtos)
    {
        authorDtos = authorDtos.ToList();
        var authorIds = authorDtos.Select(dto => dto.AuthorId).ToList();

        var specialtiesDtos = _specialtyRepository.ExecuteQuery<AuthorsSpecialtyDto>(
            $"SELECT * FROM AuthorsSpecialties WHERE AuthorId IN ({string.Join(",", authorIds)})"
        );

        var specialtiesByAuthorId = specialtiesDtos.GroupBy(dto => dto.AuthorId)
            .ToDictionary(group => group.Key, group => group.Select(s => new AuthorSpecialty(s.SpecialtyName)).ToList());

        return authorDtos.Select(dto => new Author(dto.AuthorId, dto.AuthorName, specialtiesByAuthorId.GetValueOrDefault(dto.AuthorId, new List<AuthorSpecialty>())));
    }
}