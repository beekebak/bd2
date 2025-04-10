using bd2.Application.Abstractions;
using bd2.Core.Exceptions;
using bd2.Infrastructure.DTO;
using bd2.Core.StagingAggregate;

namespace bd2.Infrastructure.Repositories;

public class AuthorRepository(GenericRepository<AuthorDto> repository, GenericRepository<AuthorsSpecialtyDto> specialtyRepository) : IAuthorRepository
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
            
        ids = ids.Distinct().ToArray();
        var authorDtos = _authorRepository.GetByIds(ids).ToList();
        if(authorDtos.Count < ids.Length) throw new EntityNotFoundException(nameof(AuthorDto));
        return MapAuthorDtosToAuthors(authorDtos);
    }

    public IEnumerable<Author> GetAll()
    {
        var authorDtos = _authorRepository.GetAll();
        return MapAuthorDtosToAuthors(authorDtos);
    }

    public int Create(Author entity)
    {
        var authorDto = new AuthorDto
        {
            AuthorName = entity.AuthorName
        };
        var id = _authorRepository.Create(authorDto);
        var createdSpecialties = entity.Specialties
            .Select(x => new AuthorsSpecialtyDto { AuthorId = id, SpecialtyName = x.SpecialtyName })
            .Select(x => _specialtyRepository.Create(x)).ToList();
        return id;
    }

    public void Update(Author entity)
    {
        var authorDto = new AuthorDto
        {
            Id = entity.Id,
            AuthorName = entity.AuthorName
        };
        _authorRepository.Update(authorDto);
    }

    public void AddSpecialty(int authorId, string specialtyName)
    {
        _specialtyRepository.Create(new AuthorsSpecialtyDto
        {
            AuthorId = authorId,
            SpecialtyName = specialtyName
        });
    }

    public void Delete(int id)
    {
        _authorRepository.Delete(id);
    }

    private Author MapAuthorDtoToAuthor(AuthorDto authorDto)
    {
        var specialtiesDto = _specialtyRepository.ExecuteQuery<AuthorsSpecialtyDto>(
            "SELECT * FROM AuthorsSpecialties WHERE AuthorId = @Id",
            new Dictionary<string, object> { { "@Id", authorDto.Id } }
        );

        var specialties = specialtiesDto.Select(s => new AuthorSpecialty(s.SpecialtyName)).ToList();

        return new Author(authorDto.Id, authorDto.AuthorName, specialties);
    }
    
    private IEnumerable<Author> MapAuthorDtosToAuthors(IEnumerable<AuthorDto> authorDtos)
    {
        authorDtos = authorDtos.ToList();
        if(!authorDtos.Any()) return [];
        var authorIds = authorDtos.Select(dto => dto.Id).ToList();

        var specialtiesDtos = _specialtyRepository.ExecuteQuery<AuthorsSpecialtyDto>(
            $"SELECT * FROM AuthorsSpecialties WHERE AuthorId IN ({string.Join(",", authorIds)})"
        );

        var specialtiesByAuthorId = specialtiesDtos.GroupBy(dto => dto.AuthorId)
            .ToDictionary(group => group.Key, group => group.Select(s => new AuthorSpecialty(s.SpecialtyName)).ToList());

        return authorDtos.Select(dto => new Author(dto.Id, dto.AuthorName, specialtiesByAuthorId.GetValueOrDefault(dto.Id, new List<AuthorSpecialty>())));
    }
}