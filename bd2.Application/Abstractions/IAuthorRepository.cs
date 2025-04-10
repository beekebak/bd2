using bd2.Core.StagingAggregate;

namespace bd2.Application.Abstractions;

public interface IAuthorRepository : IGenericRepository<Author>
{
    public void AddSpecialty(int authorId, string specialtyName);
}