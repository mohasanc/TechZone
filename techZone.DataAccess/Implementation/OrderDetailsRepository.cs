namespace techZone.DataAccess.Implementation
{
    internal class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private readonly AppDbContext _context;
        public OrderDetailsRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetails orderDetails)
        {
            _context.OrderDetails.Update(orderDetails);
        }
    }
}
