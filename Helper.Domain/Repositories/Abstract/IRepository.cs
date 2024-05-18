namespace Helper.Domain.Repositories.Abstract;

public interface IRepository<T, TId> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(TId id);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(TId id);
}