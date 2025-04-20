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

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitWork;
        public CompanyController(IUnitOfWork unitWork)
        {
            _unitWork = unitWork;
            
        }

        public IActionResult Index()
        {
            List<Company> company = _unitWork.Company.GetAll().ToList();
            return View(company);
        }
        public IActionResult Upsert(int? id)
        {
            
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company companyObj = _unitWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(companyObj);
            }


        }
        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {

            if (ModelState.IsValid)
            {
                
                if (CompanyObj.Id == 0)
                {
                    _unitWork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitWork.Company.Update(CompanyObj);
                }

                _unitWork.Save();
                TempData["success"] = "Company created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                
                return View(CompanyObj);
            }

        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });

        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyNeededDelete = _unitWork.Company.GetFirstOrDefault(u => u.Id == id);
            if (CompanyNeededDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting!" });
            }
           


                _unitWork.Company.Remove(CompanyNeededDelete);
                _unitWork.Save();
                return Json(new { success = true, message = "Deleted successfully!" });

        }

        #endregion

    }
}

