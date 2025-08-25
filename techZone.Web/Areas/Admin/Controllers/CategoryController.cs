namespace techZone.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Category.GetAllAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Category.AddAsync(category);
                await _unitOfWork.CompleteAsync();
                TempData["Create"] = "Data has created succesfully";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var category = await _unitOfWork.Category.GetFirstOrDefault(c => c.CategoryId == id);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Category.UpdateAsync(category);
                await _unitOfWork.CompleteAsync();
                TempData["Edit"] = "Data has updated succesfully";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Category.GetFirstOrDefault(c => c.CategoryId == id);

            if (category is null)
            {
                return NotFound();
            }

            await _unitOfWork.Category.RemoveAsync(category);
            await _unitOfWork.CompleteAsync();
            TempData["Delete"] = "Data has deleted succesfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
