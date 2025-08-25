namespace techZone.Entities.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> CartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
