namespace bd2.Application.Abstractions;

public interface IGenericRepository<T> where T : class
{
    T? GetById(int id);
    public IEnumerable<T> GetByIds(int[]? ids);
    IEnumerable<T> GetAll();
    void Create(T entity);
    void Update(T entity);
    void Delete(int id);
}