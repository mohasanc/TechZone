using Product = techZone.Entities.Models.Product;

namespace techZone.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> GetData()
        {
            var products = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");
            var result = products.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Description,
                p.Price,
                Category = p.Category.Name,
            });
            return Json(new { data = result });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Category.GetAllAsync();

            ProductViewModel model = new ProductViewModel()
            {
                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.CategoryId.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string imageFileName = "default.jpg";

                if (file != null && file.Length > 0)
                {
                    string rootPath = _webHostEnvironment.WebRootPath;

                    string fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(file.FileName);
                    var upload = Path.Combine(rootPath, @"images\products");

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    imageFileName = fileName + extension;
                }

                Product product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Image = imageFileName,
                    CategoryId = model.CategoryId,
                };

                await _unitOfWork.Product.AddAsync(product);
                await _unitOfWork.CompleteAsync();

                TempData["Create"] = "Data has created succesfully";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _unitOfWork.Category.GetAllAsync();

            model.CategoryList = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            });

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _unitOfWork.Category.GetAllAsync();

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Image = product.Image,
                CategoryId = product.CategoryId,
                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.CategoryId.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var productToUpdate = await _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == model.ProductId);
                if (productToUpdate == null)
                    return NotFound();

                productToUpdate.Name = model.Name;
                productToUpdate.Description = model.Description;
                productToUpdate.Price = model.Price;
                productToUpdate.CategoryId = model.CategoryId;

                if (file != null && file.Length > 0)
                {
                    string rootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(file.FileName);
                    var upload = Path.Combine(rootPath, @"images\products");

                    if (!string.IsNullOrEmpty(model.Image))
                    {
                        var oldImage = Path.Combine(rootPath, model.Image.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    productToUpdate.Image = fileName + extension;
                }

                await _unitOfWork.Product.UpdateAsync(productToUpdate);
                await _unitOfWork.CompleteAsync();

                TempData["Edit"] = "Data has updated successfully";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _unitOfWork.Category.GetAllAsync();
            model.CategoryList = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            });

            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var product = await _unitOfWork.Product.GetFirstOrDefault(c => c.ProductId == id);

        //    if (product is null)
        //    {
        //        return NotFound();
        //    }

        //    await _unitOfWork.Product.RemoveAsync(product);
        //    await _unitOfWork.CompleteAsync();
        //    TempData["Delete"] = "Data has deleted succesfully";
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Product.GetFirstOrDefault(c => c.ProductId == id);

            if (product is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.Image) && product.Image != "default.jpg")
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", product.Image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _unitOfWork.Product.RemoveAsync(product);
            await _unitOfWork.CompleteAsync();

            return Json(new { success = true, message = "Product deleted successfully" });

        }
    }
}
