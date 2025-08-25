namespace techZone.Entities.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task UpdateAsync(Product product);
    }
}
