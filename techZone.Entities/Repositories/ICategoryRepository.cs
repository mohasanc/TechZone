namespace techZone.Entities.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task UpdateAsync(Category category);
    }
}
