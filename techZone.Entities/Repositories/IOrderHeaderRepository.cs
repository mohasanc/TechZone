namespace techZone.Entities.Repositories
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateOrderStatus(int id,  string orderStatus, string paymentStatus);
    }
}
