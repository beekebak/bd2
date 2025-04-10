using bd2.Application.Abstractions;
using bd2.Core.StagingAggregate;

namespace bd2.Application.Services;

public class OriginsManagementService(IOriginRepository originRepository, IAuthorRepository authorRepository)
{
    public void AddAuthor(Author author)
    {
        authorRepository.Create(author);
    }

    public Author GetAuthor(int authorId)
    {
        return authorRepository.GetById(authorId)!;
    }

    public IEnumerable<Author> GetAuthors()
    {
        return authorRepository.GetAll();
    }

    public IEnumerable<Author> GetAuthors(int[] ids)
    {
        return authorRepository.GetByIds(ids);
    }
    
    public void UpdateAuthor(Author author)
    {
        authorRepository.Update(author);
        if(author.Specialties.Any()) author.Specialties.ForEach(specialty => authorRepository.AddSpecialty(author.Id, specialty.SpecialtyName));
    }

    public void DeleteAuthor(int authorId)
    {
        authorRepository.Delete(authorId);
    }
    
    public void AddOrigin(Origin origin)
    {
        originRepository.Create(origin);
    }

    public Origin GetOrigin(int originId)
    {
        return originRepository.GetById(originId)!;
    }

    public IEnumerable<Origin> GetOrigins()
    {
        return originRepository.GetAll();
    }

    public IEnumerable<Origin> GetOrigins(int[] ids)
    {
        return originRepository.GetByIds(ids);
    }
    
    public void UpdateOrigin(Origin origin)
    {
        originRepository.Update(origin);
    }

    public void DeleteOrigin(int originId)
    {
        originRepository.Delete(originId);
    }
}