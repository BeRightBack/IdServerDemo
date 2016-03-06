using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace IdSvrMvcClientDemo.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        } 
        // GET: /<controller>/
        public IActionResult Register()
        {
             string returnUrl = Startup.Configuration["SiteAddress"];

            
            var _Url = "https://localhost:44366/ExtAcct/Register?returnurl=" + returnUrl.ToString();
            return RedirectPermanent(_Url);
        }

        public IActionResult Manage()
        {
            string returnUrl = Startup.Configuration["SiteAddress"];


            var _Url = "https://localhost:44366/ExtMng/Index?returnurl=" + returnUrl.ToString();
            return RedirectPermanent(_Url);
        }

        public IActionResult Info()
        {
            ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                         + "before you can log in.";
            return View();
        }

        public IActionResult ConfirmEmail()
        {
            ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                         + "before you can log in.";
            return View();
        }

        public IActionResult ChangePasswordSuccess()
        {
            ViewBag.Message = "You successfully changed your password ";
            return View();
        }
        public IActionResult SetPasswordSuccess()
        {
            ViewBag.Message = "You successfully set your password ";
            return View();
        }
    }
}
