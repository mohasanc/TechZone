namespace techZone.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            int pageSize = 8;

            var products = (await _unitOfWork.Product.GetAllAsync()).ToPagedList(pageNumber, pageSize);
            return View(products);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Product.GetFirstOrDefault(
                p => p.ProductId == id,
                includeProperties: "Category"
            );

            if (product == null)
                return NotFound();

            ShoppingCart cart = new ShoppingCart
            {
                Product = product,
                ProductId = product.ProductId,
                Count = 1
            };

            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToCart(ShoppingCart shoppingCart)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            shoppingCart.ApplicationUserId = claim.Value;

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var cart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(
                c => c.ApplicationUserId == claim.Value &&
                c.ProductId == shoppingCart.ProductId
            );

            if (cart == null)
            {
                await _unitOfWork.ShoppingCart.AddAsync(shoppingCart);
                await _unitOfWork.CompleteAsync();

                HttpContext.Session.SetInt32(
                    SD.SessionKey,
                   (await _unitOfWork.ShoppingCart.GetAllAsync(sh => sh.ApplicationUserId == claim.Value)).ToList().Count()
                );
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(cart, shoppingCart.Count);
                await _unitOfWork.CompleteAsync();
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }
    }
}
