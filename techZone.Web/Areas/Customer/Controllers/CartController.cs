namespace techZone.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            foreach (var item in ShoppingCartVM.CartList)
            {
                ShoppingCartVM.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }
        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            // Check if claim exists
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return Unauthorized("Please log in first.");
            }

            // Initialize ShoppingCartVM
            ShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader()
            };

            // Fetch user data
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Set required fields
            ShoppingCartVM.OrderHeader.Name = user.Name ?? "Not Specified";
            ShoppingCartVM.OrderHeader.Email = user.Email ?? "Not Specified";
            ShoppingCartVM.OrderHeader.Address = user.Address ?? "Default Address";
            ShoppingCartVM.OrderHeader.City = user.City ?? "Not Specified";
            ShoppingCartVM.OrderHeader.Phone = user.PhoneNumber ?? "Not Specified";
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ShippingDate = DateTime.Now.AddDays(7); 
            ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;

            // Fetch cart items
            ShoppingCartVM.CartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
            if (ShoppingCartVM.CartList == null || !ShoppingCartVM.CartList.Any())
            {
                return RedirectToAction("Index", "Cart", new { area = "Customer" });
            }


            foreach (var item in ShoppingCartVM.CartList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostSummary()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            // Check if claim exists
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return Unauthorized("Please log in first.");
            }

            // Initialize ShoppingCartVM
            ShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader()
            };

            // Fetch user data
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Set required fields
            ShoppingCartVM.OrderHeader.Name = user.Name ?? "Not Specified";
            ShoppingCartVM.OrderHeader.Email = user.Email ?? "Not Specified";
            ShoppingCartVM.OrderHeader.Address = user.Address ?? "Default Address";
            ShoppingCartVM.OrderHeader.City = user.City ?? "Not Specified";
            ShoppingCartVM.OrderHeader.Phone = user.PhoneNumber ?? "Not Specified";
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ShippingDate = DateTime.Now.AddDays(7); 
            ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;

            // Fetch cart items
            ShoppingCartVM.CartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
            if (ShoppingCartVM.CartList == null || !ShoppingCartVM.CartList.Any())
            {
                return RedirectToAction("Index", "Cart", new { area = "Customer" });
            }

            // Calculate total price
            ShoppingCartVM.OrderHeader.TotalPrice = 0;
            foreach (var item in ShoppingCartVM.CartList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            // Save OrderHeader
            try
            {
                await _unitOfWork.OrderHeader.AddAsync(ShoppingCartVM.OrderHeader);
                await _unitOfWork.CompleteAsync();

                // Verify that OrderHeader was saved
                if (ShoppingCartVM.OrderHeader.Id <= 0)
                {
                    return StatusCode(500, "Failed to create OrderHeader: Id not set.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving OrderHeader: {ex.Message}");
            }

            // Save OrderDetails
            try
            {
                foreach (var item in ShoppingCartVM.CartList)
                {
                    var orderDetails = new OrderDetails
                    {
                        ProductId = item.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id, // Use OrderHeaderId
                        Price = item.Product.Price,
                        Count = item.Count
                    };
                    await _unitOfWork.OrderDetails.AddAsync(orderDetails);
                }
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving OrderDetails: {ex.Message}");
            }

            // Set up Stripe
            var domain = "https://localhost:44300/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index"
            };

            foreach (var item in ShoppingCartVM.CartList)
            {
                var sessionLineOption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineOption);
            }



            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            ShoppingCartVM.OrderHeader.SessionId = session.Id;
            await _unitOfWork.CompleteAsync();



            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(id, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                await _unitOfWork.CompleteAsync();
            }

            List<ShoppingCart> shoppingCarts =
                (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId))
                .ToList();

            await _unitOfWork.ShoppingCart.RemoveRangeAsync(shoppingCarts);
            _unitOfWork.CompleteAsync();

            return View(id);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var shoppingCart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var shoppingCart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);

            if (shoppingCart.Count <= 1)
            {
                await _unitOfWork.ShoppingCart.RemoveAsync(shoppingCart);
                var count = (await _unitOfWork.ShoppingCart.GetAllAsync(sh => sh.ApplicationUserId == shoppingCart.ApplicationUserId)).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var shoppingCart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            await _unitOfWork.ShoppingCart.RemoveAsync(shoppingCart);
            await _unitOfWork.CompleteAsync();
            var count = (await _unitOfWork.ShoppingCart.GetAllAsync(sh => sh.ApplicationUserId == shoppingCart.ApplicationUserId)).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return RedirectToAction("Index", "Home");
        }
    }
}
