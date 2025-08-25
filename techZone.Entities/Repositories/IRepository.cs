namespace techZone.Entities.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string? includeProperties = null);
        //Task<T?> GetByIdAsync(int id);
        Task<T?> GetFirstOrDefault(Expression<Func<T, bool>> predicate, string? includeProperties = null);
        Task AddAsync(T entity);
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities);
    }
}
