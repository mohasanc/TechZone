namespace techZone.DataAccess.Implementation
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task UpdateAsync(Product product)
        {
            var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
            if (productInDb != null)
            {
                productInDb.Name = product.Name;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.Image = product.Image;
                productInDb.CategoryId = product.CategoryId;
            }
        }
    }
}
