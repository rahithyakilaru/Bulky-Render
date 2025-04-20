using Bulky.Models;
using Bulky.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitWork = unitWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> product = _unitWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(product);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = CategoryList
                
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }


        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                var wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string pathName = Path.Combine(wwwRootPath, @"images\product\");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath =
                            Path.Combine(wwwRootPath, productVM.Product.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(pathName, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    };
                    productVM.Product.ImageUrl = @"images\product\" + fileName;


                }
                if (productVM.Product.Id == 0)
                {
                    _unitWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitWork.Product.Update(productVM.Product);
                }

                _unitWork.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitWork.Category
               .GetAll().Select(u => new SelectListItem
               {
                   Text = u.Name,
                   Value = u.Id.ToString()
               });
                return View(productVM);
            }

        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> product = _unitWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = product });

        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productNeededDelete = _unitWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (productNeededDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting!" });
            }
            if (!string.IsNullOrEmpty(productNeededDelete.ImageUrl))
                
                {
                    var oldImagePath =
                    Path.Combine(_webHostEnvironment.WebRootPath, productNeededDelete.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                _unitWork.Product.Remove(productNeededDelete);
                _unitWork.Save();
                return Json(new { success = true, message = "Deleted successfully!" });

        }

        #endregion

    }
}

