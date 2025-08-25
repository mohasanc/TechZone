namespace techZone.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = await _unitOfWork.OrderHeader.GetAllAsync(includeProperties: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            OrderViewModel orderViewModel = new OrderViewModel
            {
                OrderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = await _unitOfWork.OrderDetails.GetAllAsync(od => od.OrderHeaderId == id, includeProperties: "Product")
            };
            return View(orderViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderDetails(OrderViewModel orderViewModel)
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefault(oh => oh.Id == orderViewModel.OrderHeader.Id);
            if(orderHeader == null)
            {
                return NotFound();
            }

            orderHeader.Carrier = orderViewModel.OrderHeader.Carrier;
            orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = orderViewModel.OrderHeader.ShippingDate;
            orderHeader.OrderStatus = orderViewModel.OrderHeader.OrderStatus;

            _unitOfWork.OrderHeader.Update(orderHeader);
            await _unitOfWork.CompleteAsync();

            TempData["Edit"] = "Order updated successfully!";
            return RedirectToAction(nameof(Details), new { id = orderHeader.Id });
        }
    }
}
