using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bulky.Models;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customers.Controllers
{
    [Area("Customers")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitWork)
        {
            _logger = logger;
            _unitWork = unitWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }
        public IActionResult Details(int id)
        {
            ShoppingCart cart = new() {
                Product = _unitWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category"),
                Count = 1,
                ProductId = id
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;
            var cartFromDb = _unitWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == userId
            && u.ProductId == shoppingCart.ProductId);
            if(cartFromDb!= null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {

                _unitWork.ShoppingCart.Add(shoppingCart);

            }
            TempData["success"] = "Cart updated successfully!";

            _unitWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}