namespace techZone.Entities.Repositories
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        int IncreaseCount(ShoppingCart shoppingCart, int count);
        int DecreaseCount(ShoppingCart shoppingCart, int count);
    }
}
