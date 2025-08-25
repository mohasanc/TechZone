namespace techZone.DataAccess.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCart> ShoppingCart { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderHeader>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.OrderHeader)
                .WithMany()
                .HasForeignKey(od => od.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderHeader>()
                .Property(o => o.OrderDate)
                .IsRequired();

            modelBuilder.Entity<OrderHeader>()
                .Property(o => o.ShippingDate)
                .IsRequired();

            modelBuilder.Entity<OrderHeader>()
                .Property(o => o.PaymentDate)
                .IsRequired();
        }
    }
}
