namespace techZone.DataAccess.Implementation
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task UpdateAsync(Category category)
        {
            var categoryInDb = _context.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
            if (categoryInDb != null)
            {
                categoryInDb.Name = category.Name;
                categoryInDb.Description = category.Description;
                categoryInDb.CreatedAt = DateTime.Now;
            }

            await Task.CompletedTask;
        }
    }
}
