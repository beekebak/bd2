namespace bd2.Application.Abstractions;

public interface IGenericRepository<T>
{
    T? GetById(int id);
    public IEnumerable<T> GetByIds(int[]? ids);
    IEnumerable<T> GetAll();
    int Create(T entity);
    void Update(T entity);
    void Delete(int id);
}